# eShop communications series — Part 2

This snapshot builds on Part 1 and adds the first `OrderCreated` communication
vertical slice to Microsoft's .NET eShop reference application. The original
eShop README is in [eshop.README.md](eshop.README.md).

The flow is:

```text
Ordering -> Transmitly forwarding middleware -> Communications API
         -> OrderCreated pipeline -> Email -> Simulation provider
```

## Where to look

- `src/Ordering.API/Application/DomainEventHandlers/SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler.cs`
  expresses the shared `CommunicationIntents.OrderCreated` intent when an order
  is created. It dispatches to the buyer's email address, read from the access
  token on the create-order request, with the order identifier as the model.
- `src/Ordering.API/Infrastructure/Communications` holds dispatches made during
  a command until `TransactionBehavior` commits the transaction. If the
  transaction fails, the communication is dropped.
- `src/eShop.ServiceDefaults/Communications` contains the shared intent
  vocabulary, `AddEshopCommunications()` setup, transport contract, and
  forwarding middleware. Pipeline definitions do not live here.
- `src/Communications.API` exposes the generic
  `POST /api/communications/dispatch` endpoint, owns the pipeline catalog, and
  routes Transmitly logs through eShop's OpenTelemetry logging.
- `src/eShop.AppHost/Program.cs` runs and connects both services with Aspire.

Place an order in the web app. Once Ordering commits the new order, the
`OrderCreated` dispatch is forwarded to the Communications API, which composes
a simulated email. The simulation delivery report and Transmitly Debug logs
show up in the `communications-api` logs in the Aspire dashboard.
