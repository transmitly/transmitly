# eShop communications series — Part 2

This snapshot adds the first `OrderShipped` communication vertical slice to
Microsoft's .NET eShop reference application.

The flow is:

```text
Ordering -> Transmitly forwarding middleware -> Communications API
         -> OrderShipped pipeline -> Email -> Simulation provider
```

## Where to look

- `src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs`
  expresses the shared `CommunicationIntents.OrderShipped` intent using the
  buyer identity and order identifier.
- `src/eShop.ServiceDefaults/Communications` contains the shared
  intent vocabulary, `AddEshopCommunications()` setup, transport contract, and
  forwarding middleware. Pipeline definitions do not live here.
- `src/Communications.API` exposes the generic
  `POST /api/communications/dispatch` endpoint, uses Transmitly identity
  resolution to load customer profiles from Identity API, owns the pipeline
  catalog, and routes Transmitly logs through eShop's OpenTelemetry logging.
- `src/eShop.AppHost/Program.cs` runs and connects both services with Aspire.

After checkout, eShop's payment flow moves the order to `Paid`. The sample
OrderProcessor then simulates fulfillment, publishes `OrderShippingCompleted`,
and Ordering performs its existing `ShipOrderCommand`. The resulting
`OrderShippedDomainEvent` dispatches the communication intent automatically.

Communications obtains a client-credentials token and calls Identity API's
`POST /api/identities/resolve` endpoint. The response contains customer profile
data and all available email and phone addresses. Transmitly selects the
addresses needed by the configured channel; this first pipeline sends a
simulated email. The simulation delivery report and Transmitly Debug logs flow
through the normal eShop logging providers.
