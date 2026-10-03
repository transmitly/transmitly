Part 2 built the path. Ordering dispatches an `OrderCreated` intent to the buyer's email address through Transmitly, holds it until the order commits, and middleware forwards the communication context to the Communications service, where the configured pipeline decides how to fulfill it.

That first pipeline produced a deliberately bare email:

```text
Subject:
Thanks for your order!

Body:
We've received your order and we'll let you know when it ships.
```

It was enough to prove the boundary. It isn't enough to be useful to a customer.

A good order confirmation might include the customer's name, the items in the order, the amount paid, and product details. Some of that is already in hand when Ordering dispatches. The rest belongs to other services.

Part 1 gave that assembly job to Communications: Ordering supplies the identifiers and facts it owns, and Communications assembles the communication model. This article builds that step, starting with the recipient. More channels, and the policy for choosing among them, come in Part 4.

Transmitly treats the model supplied at dispatch as input to composition. Before content reaches a template, the pipeline can resolve platform identities, enrich those identity profiles, and enrich the content model itself.

At a high level, the lifecycle looks like this:

```text
DispatchAsync(...)
  |
  +-- (optional) Resolve identity references
  |       |
  |       v
  |   IPlatformIdentityProfile[]
  |
  +-- Run PlatformIdentityProfile enrichers
  |
  +-- Build dispatch/content context
  |
  +-- Run ContentModel enrichers
  |       by scope and order
  |
  +-- Render channel content
  |
  +-- Dispatch channels/providers
```

The result is a composition pipeline sitting between the business operation and the final communication.

## Starting with the transactional model

When an order is created, Ordering has just built the `Order` aggregate. eShop's aggregate holds its order items, and each `OrderItem` keeps the catalog product identifier, product name, unit price, discount, picture URL, and quantity. The order can calculate its total from those stored values.

So the order itself is a natural source for the initial transactional model.

We might project the communication-relevant data into something like this:

```csharp
public sealed record OrderCreatedModel(
    int OrderId,
    DateTime OrderDate,
    IReadOnlyCollection<OrderCreatedItem> Items,
    decimal Total);

public sealed record OrderCreatedItem(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
```

Then Ordering dispatches with the model it already has:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    [buyer],
    TransactionModel.Create(new OrderCreatedModel(
        order.Id,
        order.OrderDate,
        order.OrderItems
            .Select(item => new OrderCreatedItem(
                item.ProductId,
                item.ProductName,
                item.Units,
                item.UnitPrice))
            .ToArray(),
        order.GetTotal())));
```

That gives Communications a solid starting point without making the originating service build the final communication.

The distinction matters. The transactional model is context supplied at dispatch. The content model used for rendering keeps developing as that context moves through Communications.

```text
Ordering
    |
    | OrderCreated
    | + transactional model
    | + recipient identity
    v
Communications
    |
    | composition
    v
Enriched content context
    |
    v
Channel-specific rendering
```

How much belongs in that initial model depends on the communication.

## When should data enter the communication?

Data can enter composition at a few different points.

An originating service can supply most of the model at dispatch. That works well when the service already loaded the relevant state for the business operation. `OrderCreated` fits: Ordering already knows which products were bought, how many, and at what price.

Another communication might arrive mostly as references and lean on resolution during composition. If Communications needs data owned by another bounded context, passing a stable identifier is usually better than copying that domain's data through the originating service.

And sometimes the right model is assembled later. A communication may be delayed, accumulated with related ones, or enriched with information that changes between the business operation and rendering. Part 1's five-orders summary is that case.

These choices sit on a spectrum:

```text
More data supplied at dispatch
<---------------------------------------------->

More data resolved during composition
```

There's no prize for pushing every communication to one end.

For eShop we'll stay near the simple end. Ordering supplies the order information it already has, and Communications does two pieces of cross-service composition:

```text
Buyer identity
      |
      v
Identity resolution

Product identifiers
      |
      v
Catalog enrichment
```

Both use domains eShop already has, which makes them a good way to exercise Transmitly's enrichment workflow.

## Resolving the recipient

Ordering identifies the buyer because the order belongs to someone. How to reach that person is a different concern.

In Part 2, Ordering handed Communications an email address taken from the buyer's access token. That worked, but it meant Ordering was supplying an address. Once SMS and push arrive, the same approach would have Ordering supplying phone numbers and device tokens too, and address management would leak into the ordering domain.

We don't have to accept that. Communications can resolve the identity itself, so Ordering never has to know how a buyer is reached. The value passed to `DispatchAsync()` becomes an identity reference built from the identifier Ordering already owns:

```csharp
var buyer = new IdentityReference(CommunicationIdentityTypes.Buyer, domainEvent.UserId);
```

Ordering no longer reads the email claim at all.

When Communications processes the dispatch, Transmitly resolves that reference into one or more `IPlatformIdentityProfile` instances. Resolvers are registered by identity type:

```csharp
builder.Services.AddTransmitly(tly => tly
    // ...
    .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
    // ...
);
```

Transmitly only runs a resolver whose type matches the reference, which is why `Buyer` is a shared constant rather than a string typed in two services. eShop's resolver asks the service that owns the data. It gets a client-credentials token from Identity and calls a new `POST /api/identities/resolve` endpoint, which returns the buyer's name and every email address and phone number on the account.

Conceptually:

```text
buyer-123
    |
    v
Identity resolution
    |
    v
IPlatformIdentityProfile
    |
    +-- Name
    |
    +-- Email address
    |
    +-- SMS address
    |
    +-- Other identities
```

Today's email only needs the name and the email address. The rest matters once other channels show up, and Ordering keeps sending the same identity reference while that happens.

## Enriching the identity profile

Resolution gives us the initial `IPlatformIdentityProfile`. Profile enrichers run next, before the content itself is built.

A profile might start with very little:

```text
Platform identity
    buyer-123

Email
    ava@example.com
```

An enricher could then add recipient-oriented information:

```text
Name
    Ava Rodriguez

Locale
    en-US

Time zone
    America/Denver

Communication preferences
    Email: enabled
    SMS: enabled
```

Sources will differ between applications. Some systems get almost all of this during identity resolution. Others keep customer profiles, preference stores, tenant configuration, or communication-specific data somewhere else.

These enrichers work on the recipient. They establish who the recipient is in communications terms, how to reach them, and which recipient-specific details should feed later decisions. This is the locale and preferences row from Part 1's composition model.

eShop is the first kind of system. Identity resolution already returns the buyer's name and every address on the account, which is everything today's email needs, and eShop has no locale or preference store to draw from. So our pipeline doesn't register a profile enricher yet. The stage is there for when the recipient needs more than Identity knows.

Once profiles are resolved and enriched, Transmitly builds the content context for the pipeline.

## Enriching the transactional content

Content-model enrichment deals with the subject of the communication.

Our transactional model already has the order facts Ordering supplied:

```text
Order
    OrderId
    OrderDate
    Total

Items
    ProductId
    ProductName
    Quantity
    UnitPrice
```

The `ProductId` values give us a natural link to eShop's Catalog service.

Catalog exposes a batch endpoint that returns several items by identifier. That's handy, since an order can contain many products.

A content-model enricher can collect those identifiers:

```csharp
var productIds = order.Items
    .Select(x => x.ProductId)
    .Distinct()
    .ToArray();
```

and fetch their Catalog representations in one call:

```text
GET /api/catalog/items/by
    ?ids=1
    &ids=2
    &ids=3
```

Catalog then contributes presentation data such as description, picture data, and catalog classification. The batch endpoint returns brand and type as identifiers, so the enricher also reads Catalog's brand and type lists to get their names.

If Catalog can't be reached, the enricher logs a warning and the email goes out without product details. The order facts still come from Ordering.

Our enricher might extend each item from:

```text
ProductId
ProductName
Quantity
PurchasedUnitPrice
```

to:

```text
ProductId
ProductName
Quantity
PurchasedUnitPrice
Brand
ProductType
Description
ProductImage
```

The model gets richer without Ordering taking responsibility for Catalog's representation.

## Transactional facts and enrichment data

Bringing in Catalog data raises a semantic question.

The price stored on the order and the current Catalog price can both exist. They mean different things.

Say the customer bought an item for:

```text
$29.99
```

and Catalog now lists it for:

```text
$34.99
```

The order confirmation should show what the customer actually paid. eShop's `OrderItem` keeps the unit price from when the item was added to the order, alongside its product identifier and other transactional values.

Catalog adds context. It doesn't redefine the transaction.

That suggests a rule for enrichment:

> An enricher should understand the semantics of the model it is enriching. Current data can supplement transactional data, refresh it when the communication requires current state, or participate in a new derived representation.

Which behavior fits depends on the communication.

For `OrderCreated`, the purchased quantity and price describe the transaction. Current Catalog data layers richer product presentation on top.

Another pipeline might need something quite different.

## From dispatch to composed content

With identity resolution and Catalog enrichment in place, the full composition path looks like this:

```text
Ordering
    |
    | DispatchAsync("OrderCreated", ...)
    |
    | Transactional model
    |   OrderId
    |   OrderDate
    |   Items
    |       ProductId
    |       ProductName
    |       Quantity
    |       UnitPrice
    |   Total
    |
    | Recipient
    |   BuyerIdentityId
    v
Communications
    |
    v
Resolve BuyerIdentityId
    |
    v
IPlatformIdentityProfile[]
    |
    v
PlatformIdentityProfile enrichers
    (none needed for eShop yet)
    |
    v
Build dispatch/content context
    |
    v
ContentModel enrichers
    |
    +-- Batch resolve ProductIds from Catalog
    |
    +-- Add product presentation data
    |
    v
Enriched OrderCreated content context
    |
    v
Render channel content
```

The application supplied an intent, a recipient reference, and some transactional context. The pipeline turned those into a model built for communication.

## A purpose-built content model

By the time rendering starts, the content context holds information from several sources:

```csharp
public sealed record OrderCreatedContentModel(
    RecipientModel Recipient,
    int OrderId,
    DateTime OrderDate,
    decimal Total,
    IReadOnlyCollection<OrderCreatedContentItem> Items);

public sealed record RecipientModel(
    string? FirstName);

public sealed record OrderCreatedContentItem(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal PurchasedUnitPrice,
    string? Brand,
    string? ProductType,
    string? Description,
    string? ProductImage);
```

Broken down by where each piece came from:

```text
Ordering
    OrderId
    OrderDate
    ProductName
    Quantity
    PurchasedUnitPrice
    Total

Identity
    FirstName
    communication addresses

Catalog
    Brand
    ProductType
    Description
    product presentation data
```

Neither the template nor the channel provider has to reconstruct that composition. Data spread across the application has already been turned into one coherent communication context.

## Why make enrichment part of the pipeline?

We could do all of this before calling Transmitly.

Ordering could resolve the customer, call Catalog, build a large content model, pick an address, and submit a finished message.

That's the leak Part 1 described. Ordering slowly turns into an orchestrator for data that exists mostly to produce communications, just because it happened to create the intent.

Transmitly gives that composition an explicit lifecycle instead:

```text
Dispatch
   |
   v
Identity resolution
   |
   v
Identity enrichment
   |
   v
Content enrichment
   |
   v
Channel rendering
   |
   v
Delivery
```

The `OrderCreated` pipeline can evolve around this lifecycle while Ordering keeps making the same call:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    [buyer],
    TransactionModel.Create(model));
```

A new requirement for product metadata goes in a content enricher. More recipient information comes from an identity-profile enricher. Every channel in the pipeline then reuses the resulting context.

Composition becomes part of the communications strategy, not one more responsibility scattered across every service that creates a communication.

## Enrichers also give us control over timing

So far, the example uses Ordering's transactional model and adds Catalog data.

That's the simplest useful implementation. But supplying a model to `DispatchAsync()` doesn't commit us to rendering those exact values.

Composition might happen later.

Imagine an ordering system that lets customers modify an order for a while after placing it. Over a few minutes a customer might:

```text
Place order
    |
Add item
    |
Change quantity
    |
Remove item
    |
Update payment
```

Turning each of those into a notification produces a stream of messages that's technically accurate and practically annoying.

A communication policy could instead accumulate the related activity for a window, much like Part 1's five-orders summary:

```text
OrderPlaced -------\
ItemAdded ----------\
QuantityChanged -----> accumulated communication
ItemRemoved --------/
PaymentUpdated ----/
                         |
                         | N-minute window
                         v
                  ready to compose
```

When the accumulated communication is ready, Communications might decide the most useful thing to show is the current order, not the sequence of models originally supplied.

An order content enricher can take the `OrderId`, fetch the latest authoritative state, and update the content model before rendering:

```text
Accumulated context
       |
       | OrderId
       v
Order enricher
       |
       v
Current order state
       |
       v
One composed communication
```

The original dispatch models still mattered. They described the operations that triggered the communication and carried the data available at each point. The communication policy decides what's finally presented.

Which leads to the core distinction:

> **Dispatch captures the context that caused a communication. Composition determines the information ultimately presented to the recipient.**

Sometimes those two models are nearly identical. Sometimes composition adds a few properties. A delayed or accumulated workflow may refresh much more before rendering.

The pipeline handles each case.

## Resolving at composition time is another valid starting point

The same reasoning explains why another application might dispatch a much smaller model from the start.

Instead of:

```text
OrderCreated
    OrderId
    OrderDate
    Items
    Total
```

it could send:

```text
OrderCreated
    OrderId
```

and let an order enricher resolve the full state later.

That can make sense when the source domain exposes a stable historical representation, when communications are routinely delayed, or when current state matters more than saving a lookup.

Our eShop implementation doesn't need that. The order is already in hand when it's created, so passing a useful transactional model keeps the example direct.

The architecture leaves the choice open for intents that need different semantics.

## Direct calls versus local projections

The Catalog enricher can start with the simplest data access: call Catalog during composition.

```text
ContentModel enricher
       |
       v
Catalog API
       |
       v
Catalog items
```

For this example's volume and purpose, that's enough.

A larger communications system might get enricher data differently. Communications could keep a local projection fed by Catalog integration events:

```text
Catalog
    |
    | integration events
    v
Communications product projection
    |
    v
Catalog enricher
```

From the pipeline's point of view, the enrichment stage doesn't change. Only the enricher's data source does.

Recipient profiles can evolve the same way. A platform identity resolver might call an identity service directly at first, then later resolve from communications-owned data synchronized from that service.

So the architecture grows in response to real latency, availability, and throughput needs instead of demanding those mechanisms in the first implementation.

## Rendering comes last

Once identity resolution and enrichment finish, the Communications service has one shared context that each channel can use to express the intent.

This is where the split between **communication intent** and **channel content** pays off.

Our intent is still:

```text
OrderCreated
```

The enriched model might contain:

```text
OrderCreatedContentModel

Recipient
    FirstName
    Email
    MobileNumber
    PushIdentity

Order
    OrderId
    Total

Items
    ProductName
    Quantity
    PurchasedUnitPrice
    Brand
    Description
```

Email has room to show most of that directly. A code-defined email template might render a summary of the order:

```text
Subject:
Thanks for your order #123

Hi Ava,

We've received your order. We'll let you know when it ships.

2 × .NET Bot Black Hoodie
    AdventureWorks Apparel
    $42.00 each

1 × .NET Mug
    AdventureWorks
    $14.99

Order total: $98.99

View your order:
https://eshop.example/user/orders
```

That's the email the eShop sample renders today. The other channels come in Part 4, but here's how differently they'd use the same context.

SMS has different constraints. Repeating the full email would waste the channel, so the same `OrderCreated` pipeline could define a much shorter SMS:

```text
We've received your eShop order #123.
View your order: https://eshop.example/o/123
```

Push can be shorter still, because the notification itself can open the app:

```text
Title:
Thanks for your order

Body:
Order #123 has been received.

Action:
Open order 123 in the eShop app
```

All three express the same communication:

```text
                         OrderCreated
                              |
                    enriched content context
                              |
              +---------------+---------------+
              |               |               |
              v               v               v
            Email            SMS             Push
              |               |               |
       detailed summary    short link      brief alert
       and order link      to order        + app action
```

Ordering didn't have to make any of those choices.

Communications knows things Ordering doesn't need to reason about: which addresses the recipient profile has, which channels the recipient has enabled, how much content suits each medium, whether a channel can link into the app, and which providers are configured to deliver it.

Those factors shape the communication strategy independently of the intent.

For one customer, `OrderCreated` might mean email only:

```text
OrderCreated
    |
    +-- Email
```

Another might prefer SMS:

```text
OrderCreated
    |
    +-- SMS
```

A mobile-app user might get push immediately and a richer email for reference:

```text
OrderCreated
    |
    +-- Push
    |
    +-- Email
```

The content differs because the channels serve different purposes. The enriched context stays shared.

That's what moving composition and channel policy into Communications buys us. Ordering doesn't dispatch `OrderCreatedEmail`, `OrderCreatedSms`, or `OrderCreatedPush`, the kind of channel-bound name Part 1 warned against.

It dispatches:

```csharp
await communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    [buyer],
    TransactionModel.Create(model));
```

Communications decides how `OrderCreated` is expressed for that recipient under the current policy. For an imperative like the hypothetical `PasswordReset` from Part 2, the pipeline fixes those choices instead of leaving them to policy. Resolution, enrichment, and rendering work the same way either way.

## The Communications domain is taking shape

Our `OrderCreated` implementation now has several stages:

```text
Ordering
    |
    | business operation
    v
OrderCreated intent
    |
    | transactional context
    | recipient reference
    v
Communications
    |
    +-- identity resolution
    |
    +-- identity-profile enrichment (available, unused for now)
    |
    +-- content-model enrichment
    |
    +-- channel-specific rendering
    |
    +-- provider dispatch
```

Each stage adds information or applies policy at the point where it's useful.

Ordering contributes the context it already has when the order is created. Identity resolution turns the buyer reference into a recipient Communications can address, and for now it also supplies everything the email needs to know about the buyer. Profile enrichers can add recipient information later. The Catalog content enricher uses the product identifiers already on the order to pull presentation data through eShop's existing batch Catalog API.

Every configured channel then gets that context, so email, SMS, push, or anything else can present the same intent in the form that suits it.

## Where we ended up

The transactional model passed to `DispatchAsync()` gave `OrderCreated` a head start. Ordering already had the order loaded, so we projected the relevant state instead of loading it again somewhere else.

The rest of the model was built inside Communications:

```text
DispatchAsync(...)
       |
       | Order transactional model
       | Buyer identity reference
       v
Resolve identities
       |
       v
IPlatformIdentityProfile[]
       |
       v
Build content context
       |
       v
Catalog ContentModel enricher
       |
       | ProductId -> richer product information
       v
Enriched content context
       |
       +-------- Email template
       |
       +-------- SMS template   (Part 4)
       |
       +-------- Push content   (Part 4)
```

Other communications can start differently. One might arrive with little more than identifiers and resolve most of its state during composition. A delayed one might refresh before rendering, and a high-volume system might feed its enrichers from local projections. An accumulated one might combine several operations and compose a single message from the latest state.

They all fit the same pipeline, because the transactional model is an input to composition, not a declaration that composition is finished.

For eShop, Identity and Catalog are enough to show the mechanics without inventing services the application doesn't have. Together they cover the two main kinds of enrichment: **who we're communicating with** and **what we're communicating about**.

Once that context exists, each channel can use it differently. Today, email gives the detailed order summary. When SMS and push join the pipeline, SMS will link back to the order and push will take the customer straight to the right screen in the app.

The business intent stays the same. The communication strategy is what changes.

---

**Next: [Part 4, One Event, Multiple Channels, Multiple Providers](../part%204/article.md).** The same intent goes out as an email, an SMS, and a push notification, with the simulator standing in for SMTP, Twilio, and Firebase.
