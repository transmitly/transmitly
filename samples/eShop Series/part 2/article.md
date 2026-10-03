Part 1 ended with a plan: add Transmitly to Microsoft eShop, create a new boundary around an order communication, and start with simulated delivery.

This article adds an `OrderCreated` communication. The first version produces an email, but Ordering's code only ever deals with the intent.

The flow is simple:

```text
Ordering
    |
    | OrderCreated
    v
Communications
    |
    | communication policy
    v
Email
```

Transmitly provides the abstraction on both sides of this boundary. That matters. eShop is distributed today, but the same application code could just as easily run a Transmitly pipeline locally. Moving communication processing into another service becomes a configuration and middleware concern, not a new programming model.

## Expressing the communication from Ordering

eShop already has a well-defined order-creation operation. When a buyer checks out, Ordering creates the order and owns that state, so it has the business context to say an `OrderCreated` communication intent now exists.

At the application level, we want that call to stay small:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    buyerEmail,
    new { orderId = domainEvent.Order.Id },
    cancellationToken: cancellationToken);
```

What matters here is the vocabulary.

Ordering expresses an intent to communicate with `OrderCreated`, not a delivery mechanism. The configured pipeline decides what that means operationally.

Ordering makes this call itself, inside `SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler`. It handles `OrderStartedDomainEvent`, the domain event eShop raises when a new order is created. Another handler for the same event, `ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler`, publishes `OrderStatusChangedToSubmittedIntegrationEvent`. Communications could subscribe to that integration event and guess that a message is warranted, but as Part 1 argued, that's the wrong way around. The integration event tells other services a fact. The dispatch tells Communications that Ordering has decided there's an intent to communicate, and it's up to Communications to decide how and when to communicate, if at all.

`buyerEmail` comes from the `email` claim on the access token that arrives with the create-order request. Ordering reads it for this one call and doesn't store it. That keeps the first slice small, but it's a shortcut, and we'll come back to it.

In a smaller application, the whole pipeline could be registered in the same host _(Program.cs)_:

```csharp 
var communicationsClient = new CommunicationsClientBuilder()
    .AddSimulationSupport()
    .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
    {
        pipeline.AddEmail(
            "orders@eshop.local".AsIdentityAddress("eShop"),
            email =>
            {
                email.Subject.AddStringTemplate(
                    "Thanks for your order!");

                email.TextBody.AddStringTemplate(
                    "We've received your order and we'll let you know when it ships.");
            });
    })
    .BuildClient();
```

Calling `DispatchAsync()` triggers our `OrderCreated` pipeline, composes the configured email, and hands it to the simulation provider.

When communication requirements are local to one application, that's a perfectly reasonable deployment model.

eShop gives us a reason to go further.

## Moving pipeline execution into Communications

Once several services need transactional communications, centralizing composition and delivery starts to pay off.

Ordering may need `OrderCreated`, `OrderShipped`, and `OrderCancelled`. Identity may need `PasswordReset`. Payment may eventually need `PaymentFailed`. Over time each of those can pick up templates, localization, recipient preferences, multiple channels, and provider-specific behavior.

Instead of spreading that configuration across every service, we move pipeline configuration and management into a dedicated Communications service.

Even with a whole new service in the picture, our application code _still_ doesn't change. Transmitly's middleware gives us a seam between dispatching an intent and processing it.

The path becomes:

```text
                    Ordering
                       |
                       | DispatchAsync(
                       |   "OrderCreated",
                       |   buyerEmail,
                       |   { orderId })
                       v
                    Transmitly
                       |
                 client middleware
                       |
                       v
              Communications API
                       |
                    Transmitly
                       |
                  OrderCreated
                    pipeline
                       |
                       v
                     Email
```

The middleware receives the communication context created during dispatch and forwards what the Communications service needs to continue processing it.

[Transmitly's Microservices sample](https://github.com/transmitly/transmitly/tree/main/samples/Microservices) uses the same pattern. Application code keeps working through `ICommunicationsClient`, and middleware decides that processing continues across a service boundary.

## Sharing the client configuration

In an app like eShop, several services will probably need the same remote communications capability. Repeating the transport configuration in each project just invites drift, so this is a natural spot for a shared extension.

In an application project, configuration can be as small as:

```csharp
builder.Services.AddEshopCommunications();
```

That extension configures Transmitly and registers the middleware that forwards communications to the central service.

Conceptually:

```text
AddEshopCommunications()
        |
        +-- register Transmitly
        |
        +-- add forwarding middleware
        |
        +-- configure Communications endpoint
```

Ordering, Identity, Payment, and any other service can use the same extension and keep their own communication vocabulary.

Their application code still depends on:

```csharp
ICommunicationsClient
```

and dispatches intents the same way:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    buyerEmail,
    new { orderId = domainEvent.Order.Id });
```

Shared code holds the stable intent names used across the transport boundary, not their pipeline definitions. `CommunicationIntents` publishes the intents available to everyone. Composition, channels, templates, and delivery policy stay with Communications.

This pays off if the deployment architecture changes later. A service could process pipelines locally in one environment and forward them remotely in another.

## Waiting for the order to commit

There's a timing problem hiding in that handler. eShop runs domain event handlers before it saves anything: `OrderingContext.SaveEntitiesAsync` dispatches domain events, then calls `SaveChanges`, and `TransactionBehavior` commits the transaction after the command handler returns. Integration events cope with this through eShop's outbox. They're saved inside the transaction and published only after the commit.

A dispatch sent straight from the handler doesn't get that protection. It goes out while the transaction is still open. If the commit then fails, the buyer gets a confirmation for an order that doesn't exist.

The fix uses the same seam as forwarding: client middleware. Ordering registers one more middleware next to the shared setup:

```csharp
builder.Services.AddEshopCommunications();
builder.Services.AddSingleton<ICommunicationClientMiddleware, TransactionalCommunicationsMiddleware>();
```

`AddEshopCommunications()` adds any registered middleware after the forwarding middleware, so it sees each dispatch first. `TransactionBehavior` opens a scope around each transaction and flushes it once the commit and the integration events are done:

```csharp
using var pendingCommunications = PendingCommunications.Begin();

// ... begin, handle the command, commit ...

await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
await pendingCommunications.FlushAsync(_logger, CancellationToken.None);
```

While the scope is open, the middleware holds each dispatch instead of forwarding it. If the transaction fails, the scope is disposed without a flush and the dispatch is dropped. The handler still calls `communicationsClient.DispatchAsync()` exactly as before. It doesn't know the dispatch was deferred.

Held dispatches live in memory. If the process stops between the commit and the flush, the communication is lost. A durable outbox for communication intents is possible, but it's outside the scope of this series.

## A general dispatch boundary

The Communications service's HTTP surface can follow the same principle.

Every application performs the same operation: dispatch a communication intent along with the context needed to process it.

```text
POST /api/communications/dispatch
```

The dispatch context carries the intent, the recipients, the transactional model, any dispatch options, and metadata about the dispatch itself. In the shared contract that's `CommunicationDispatchRequest`:

```text
CommunicationDispatchRequest

    Intent
        OrderCreated

    Recipients
        alice@example.com

    Model
        orderId: 123

    AllowedChannels / PipelineId / Culture
        (empty: the pipeline decides)

    Metadata
        CorrelationId   trace id of the create-order request
        Source          Ordering.API
        DispatchedAt    2026-10-01T16:04:12Z
        Properties      baggage from the current activity
```

The forwarding middleware fills in the metadata, so application code doesn't pass any of it. The correlation id reuses the current trace id, which lets one dispatch be followed through both services' logs. Properties come from the activity's baggage, so a service can attach ambient context, such as a tenant, without changing the `DispatchAsync()` call.

When it arrives, the Communications service hands that context back to Transmitly, which resolves the intent against the configured pipeline catalog.

So the transport API stays stable as the catalog grows:

```text
OrderCreated
OrderShipped
OrderCancelled
PasswordReset
PaymentFailed
WelcomeCustomer
```

Each intent is a different communication policy. They all enter Communications through the same dispatch operation.

That gives the service a clean shape. The HTTP layer transports communication context. Transmitly handles data resolution, composition, and dispatch strategy.

## Defining the pipeline catalog

For the first implementation, we define the pipeline directly in code:

```csharp
builder.Services.AddTransmitly(tly => tly
    .AddSimulationSupport()
    .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
    {
        pipeline.AddEmail(
            "orders@eshop.local".AsIdentityAddress("eShop"),
            email =>
            {
                email.Subject.AddStringTemplate(
                    "Thanks for your order!");

                email.TextBody.AddStringTemplate(
                    "We've received your order and we'll let you know when it ships.");
            });
    }));
```

Looks familiar, doesn't it? It's the same pipeline from the single-host example, now living in Communications. That gives `OrderCreated` a concrete meaning inside the service.

```text
OrderCreated
     |
     v
   Email
     |
     v
Simulation Provider
```

Static configuration works well for a sample because the whole policy fits in a few lines of code. It's also enough for plenty of production applications.

More complex systems may eventually want pipeline configuration to come from somewhere else.

A database might hold tenant-specific communication rules. Templates might live in a CMS, and localization in a content service. Or administrators might need to change communication policy without redeploying the Communications service.

Those are all different implementations of the same responsibility:

```text
                    Communications
                          |
                          v
                  Pipeline Resolution
                          |
             +------------+------------+
             |            |            |
             v            v            v
           Code        Database       CMS
             \            |            /
              \           |           /
               +----------+----------+
                          |
                          v
                 Communication Pipeline
                          |
                   +------+------+
                   |      |      |
                 Email   SMS    Push
```

Transmitly's extensibility leaves the Communications domain room to grow in that direction without touching the services that dispatch intents.

So the real decision isn't where pipeline configuration is stored. It's where responsibility for the pipeline lives.

## Why use Transmitly on the client side?

At first glance, the originating service needs very little. We could write an `HttpClient`, serialize an intent, and post it to `/api/communications/dispatch`.

For a single application with a fixed distributed architecture, that might be enough.

Transmitly gives us a broader abstraction.

The same application call:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    buyerEmail,
    new { orderId = domainEvent.Order.Id });
```

works in either of these deployments:

```text
Local processing

Application
    |
    v
ICommunicationsClient
    |
    v
Pipeline
    |
    v
Provider
```

or:

```text
Remote processing

Application
    |
    v
ICommunicationsClient
    |
    v
Forwarding Middleware
    |
    v
Communications Service
    |
    v
Pipeline
    |
    v
Provider
```

The application is coupled to the communication concept, not to where that communication gets processed.

That matters as a system changes.

An application can start with local SMTP and a handful of pipelines. As communications grow, a shared extension can configure the same client to forward dispatches to a central service. Pipeline definitions then move into Communications, along with templates, recipient resolution, delivery reporting, and channel policy.

The business code that created those intents doesn't have to follow that migration.

That's the practical reason to use Transmitly on both sides. One communications programming model, while middleware, pipeline configuration, and deployment topology evolve around it.

## Keeping the communication context meaningful

The generic dispatch endpoint works because the payload is a communication intent, not a prebuilt message.

In this first slice, Ordering supplies the recipient as an email address, along with a model containing information it owns:

```text
OrderCreated

Recipient
    alice@example.com

Model
    orderId: 123
```

Communications uses that context to build whatever the configured pipeline needs. Today that's one email.

This is the shortcut from earlier. By passing an address, Ordering has taken on a small piece of address management. One email claim from a token is easy to live with. But when SMS arrives, the same buyer also needs a phone number:

```text
+1 555 ...
```

and push delivery needs one or more device tokens. None of that belongs in Ordering. Ordering knows the buyer as an identity, and knowing how to reach that identity on each channel is Communications' job.

In Part 3, Ordering stops passing an address. It dispatches the buyer's identity, and Communications resolves that identity to the addresses each channel needs. The intent and the model stay the same. The recipient becomes an identity reference instead of an address.

Once recipients are resolved inside Communications, an `OrderCreated` pipeline can grow from:

```text
OrderCreated
     |
     v
   Email
```

to:

```text
OrderCreated
     |
     +----> Email
     |
     +----> SMS
```

without changing anything Ordering sends.

Recipient resolution, channel availability, and communication preferences all become part of the communications model.

## Building the first vertical slice

For this article, we keep the first pipeline deliberately small.

Ordering dispatches:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    buyerEmail,
    new { orderId = domainEvent.Order.Id });
```

Once the order commits, Transmitly middleware forwards that communication context to:

```text
POST /api/communications/dispatch
```

The Communications service resolves:

```text
OrderCreated
```

and finds the pipeline we configured earlier:

```text
OrderCreated
     |
     v
Email
     |
     v
Simulation
```

The simulation provider gives us a complete dispatch path without bringing an external provider into the example.

```text
┌────────────────────────────┐
│          Ordering          │
│                            │
│ Order created              │
│      |                     │
│      v                     │
│ Dispatch("OrderCreated")   │
└─────────────┬──────────────┘
              |
              v
    Transmitly Middleware
              |
              v
  /api/communications/dispatch
              |
              v
┌────────────────────────────┐
│       Communications       │
│                            │
│ Pipeline Catalog           │
│      |                     │
│      +-- OrderCreated      │
│              |             │
│              v             │
│            Email           │
│              |             │
│              v             │
│          Simulation        │
└────────────────────────────┘
```

Once that path works, swapping simulation for SMTP or SendGrid is a provider configuration change inside Communications.

The more interesting change is adding a channel. If Product decides new orders should also send an SMS, the pipeline grows and Ordering keeps dispatching the same `OrderCreated` intent.

That's where the intent abstraction starts earning its keep.

## Adding another service

The design gets clearer once another part of eShop needs communications.

Say Identity needs to send password-reset communications.

> `PasswordReset` isn't part of this series' code. It's here to show how a second service would plug into the same boundary.

Identity would use the same shared setup:

```csharp
builder.Services.AddEshopCommunications();
```

and the same application abstraction:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.PasswordReset,
    recipient,
    model);
```

The context would travel through the same dispatch endpoint to the same Communications service.

The catalog would then contain:

```text
OrderCreated
PasswordReset
```

Each pipeline would have its own composition and channel policy. Both originating services would use the same communication model.

`PasswordReset` isn't quite like `OrderCreated`, though. In Part 1's terms it's closer to an imperative. Identity can't let channel policy or customer preferences decide where a reset link goes; it has to reach the verified address on the account. So the `PasswordReset` pipeline would be locked to that channel and recipient, while `OrderCreated` stays free to grow. The dispatch call would look the same either way. The difference lives in how Communications configures each pipeline.

As the application grows, the architecture ends up looking like this:

```text
Ordering --------\
                  \
Identity ----------> Communications
                  /        |
Payment ----------/         v
                     Pipeline Catalog
                      /     |      \
                     /      |       \
            OrderCreated PasswordReset ...
```

The shared code stays small because it defines how to reach Communications, not what Communications knows how to send.

That keeps the catalog centralized without making every application depend on a catalog library.

> The communications client and the applications don't have to evolve in lockstep. Developers can dispatch intents before Communications has configured them, and the communications team can wire up templates, channels, and delivery strategy on its own schedule.

## Where we ended up

The requirement most applications start with is easy to state:

> Send an email when an order is placed.

The implementation gives the application a more durable concept:

```text
OrderCreated
```

Ordering dispatches that intent through `ICommunicationsClient`, and Ordering's own middleware holds it until the order commits. Shared Transmitly configuration adds middleware that forwards the communication context to a generic dispatch endpoint. The Communications service resolves `OrderCreated` against its pipeline catalog and, for now, produces one simulated email.

The design separates three concerns:

```text
Application
    expresses communication intent

Transmitly infrastructure
    determines where it is processed

Communications
    determines how it is composed and delivered
```

In eShop, processing happens in a dedicated service. A smaller application could use the same Transmitly API with locally configured pipelines. The Communications service defines its catalog in code today. A more advanced implementation could pull pipelines, templates, or other policy from a database, a CMS, or some other configuration system.

Those choices can change independently because the business-facing abstraction stays the same:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    buyerEmail,
    new { orderId = domainEvent.Order.Id });
```

We have an email in the system now. The bigger result is the communications boundary around it.

The one piece of that call that won't last is `buyerEmail`.

---

**Next: [Part 3, Model Intent, Not Delivery](../part%203/article.md).** Ordering stops passing an email address, and Communications resolves the buyer through Identity and enriches the order with product details from Catalog.
