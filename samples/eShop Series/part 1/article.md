Almost every application sends communications. Most of us call them notifications: order confirmations, password resets, account created, shipping updates, security alerts. They're so ordinary that, while an application is new, nobody treats them as a separate architectural concern.

They usually start like this:

```csharp
await emailClient.SendAsync(...);
```

Maybe the application uses an SMTP server. Maybe it starts with a provider such as SendGrid. Either way the requirement is simple. Something happened, and the business rules say somebody needs to hear about it.

For a small application this is often the right call. If the system sends a handful of emails and everything needed to build them is already in the same process, a separate communications architecture is a large infrastructure investment without immediate payoff.

Microsoft's .NET eShop reference application isn't small, and that makes it a good place to study the problem. It's already split along business contexts. Ordering owns orders. Identity owns users and authentication. Payment processing runs as its own service and reports outcomes back through integration events. The services talk through deliberate contracts instead of sharing one convenient object graph.

eShop doesn't send customer communications today. If we added an order confirmation the way most applications start, we'd inject an `IEmailClient` into Ordering and call it a day.

That's where something that looks trivial starts working against the boundaries eShop was built around.

This series adds a communications service to eShop. Before writing any code, we'll work out where the service's boundary belongs and why.

## An order communication crosses domain boundaries

Take the confirmation we send after an order is created. Ordering looks like the obvious owner. It knows the order exists, and it holds the order number, line items, quantities, and the rest of the ordering data.

But a good order communication might also include the recipient's name and email address, payment information, shipping details, localization settings, or communication preferences. Each of those has a natural owner, and that owner isn't necessarily Ordering.

eShop demonstrates these boundaries well. Ordering has its own model of a buyer, shaped for ordering. It keeps an identity identifier and whatever else Ordering needs to do its work. That model doesn't need to grow into a full representation of the user's identity just because we eventually want to send a message.

Payment works the same way. It has its own lifecycle and sends relevant outcomes across a boundary. Ordering reacts to those outcomes and keeps ownership of its own state.

This is one of the practical payoffs of Domain-Driven Design. A bounded context doesn't try to model the whole business, only what it needs to meet its own responsibilities.

Ordering understands a buyer in terms of placing and managing orders. Identity understands the same person differently. Payment sees the transaction through yet another model. These representations can coexist without collapsing into one universal `Customer` object shared by every component in the system.

Communications complicate this, because a message often needs a view of the business that spans several of those boundaries.

An order confirmation isn't an `Order` serialized into HTML. It's a representation, built for a recipient, of something that happened in the business. Building it may take data from Ordering, Identity, Payment, Shipping, and other contexts.

Once you see that, the ownership problem gets clearer. The order may be the reason a communication exists. The communication still has its own model and its own lifecycle.

## When composition leaks back into the originating domain

Say we make Ordering responsible for the whole communication. At first it needs only an order and an email address. Over time the message gets more useful, and more useful means more information.

Customer service wants the payment status included. Product wants an estimated delivery date. Marketing wants localized copy. Later, customers get to choose between email, SMS, and push notifications.

The flow starts to look like this:

```text
Order Created
     |
     v
 Ordering
     |
     +------> Identity
     |         recipient information and preferences
     |
     +------> Payment
     |         payment state
     |
     +------> Shipping
     |         delivery information
     |
     v
 Compose Communication
     |
     v
 Render Content
     |
     v
 Select Channel / Provider
     |
     v
 Deliver
```

There's rarely a single moment when this feels obviously wrong. Every new dependency shows up attached to a reasonable requirement. We need the recipient's email address, so we fetch it. We need the payment status, so we fetch that too. Eventually Ordering has become an orchestrator for data and behavior that exist mostly to produce customer communications.

The dependency is easy to miss because the communication really is about an order. The question is what each domain is responsible for knowing.

Ordering should know that an order was created, paid, cancelled, or shipped. It should know the identifiers and facts it needs to perform ordering behavior. That doesn't mean it should also know how those facts are presented to a recipient, where the recipient's addresses come from, which channels are appropriate, or how delivery works.

Those concerns are a domain of their own.

## What eShop's existing boundaries already show

eShop already has the kind of separation we want to keep. Information moves between parts of the application through deliberate contracts, so no domain has to understand another domain's internals.

Integration events are one way eShop does this. A payment result can cross a service boundary as a fact, letting Ordering update the state it owns. An order status change can be published the same way, for any consumer with its own reason to care.

That says something about boundaries. It doesn't mean communications should become one more passive integration-event subscriber.

A communication intent means something different.

An integration event says something happened and makes that fact available to whoever is interested. A communication intent says the service that owns a business operation has decided there is now something to communicate.

The two may come from the same operation. They serve different purposes.

Take an order that has just been created:

```text
                    Ordering
                       |
                 Order is created
                       |
            +----------+----------+
            |                     |
            v                     v
    Integration Event      Communication Intent
            |                     |
            v                     v
      Other Services        Communications
```

Ordering owns the business operation, so it's the natural place to decide that the operation creates a communication intent.

It doesn't have to wait for a communications service to notice an event and guess that a message would probably help. The business behavior can be explicit: this operation produces an `OrderCreated` communication intent.

From there, the intent belongs to the communications domain.

That gives us the boundary:

> The service that owns the business operation creates the communication intent. The communications domain owns the lifecycle of that intent.

## Communication intent describes purpose

The word *intent* separates the reason for communicating from the mechanism that fulfills it.

Suppose Ordering dispatches an intent named:

```text
OrderCreated
```

From Ordering's side, that intent can stay small: an order identifier, the buyer's identity identifier, a payment identifier, and anything else Ordering legitimately owns and considers relevant to the communication.

Conceptually:

```text
Ordering
    |
    | OrderCreated
    | orderId: 123
    | paymentId: xyz
    | recipientId: abc
    | itemCount: 4
    v
Communications
```

Ordering has said why the communication exists. Communications now has enough to start fulfilling it.

Today's communication policy might produce a single email:

```text
OrderCreated
     |
     v
   Email
```

As the product evolves, the same intent might produce both email and SMS:

```text
OrderCreated
     |
     +----> Email
     |
     +----> SMS
```

Later, channel selection might depend on recipient preferences, geography, account type, urgency, or other communication-specific rules.

None of those changes alter the intent of communicating that an order was created. They change the organization's communication policy around that event.

So Ordering has no reason to bake today's delivery strategy into an abstraction like `SendOrderConfirmationEmail`. That name welds together two decisions that change for different reasons: why we are communicating, and which channel we happen to use right now.

`OrderCreated` is the more durable vocabulary. It states the business purpose and leaves the communications domain to decide what that purpose means operationally.

## Communications becomes a domain of composition

Once an intent crosses into Communications, the problem shifts from managing business state to composing a communication.

An `OrderCreated` communication might need a model like this:

```text
Order
    Ordering

Recipient
    Identity

Payment Status
    Payment

Delivery Information
    Shipping

Locale / Preferences
    Profile
```

The communications domain can build that model however suits the architecture. Some information gets resolved when the communication is composed. Some may already sit in a communications-oriented projection. Integration events can supply facts that Communications keeps for later. In more complex systems, an intent might be enriched gradually as related information arrives.

Which approach fits depends on latency, consistency, resilience, and how much data is involved. These are implementation details inside the larger ownership model.

Ordering supplies the identifiers and facts it owns. Communications assembles the communication model.

That keeps cross-domain composition where it belongs.

It also gives Communications a coherent set of responsibilities. Recipient resolution, message composition, localization, channel policy, template rendering, provider selection, delivery reporting, scheduling, suppression, retries, and auditing all concern the lifecycle of a communication, not the lifecycle of an order.

Where Communications physically runs can vary. In one application it's a module; in another it's an independently deployed service. The architectural benefit comes from treating it as a separate responsibility before deciding where the code lives.

## Intent has a lifecycle

One consequence of this model: a communication intent doesn't have to mean immediate delivery.

When communication code sits right beside the business operation, we drift toward a one-to-one relationship:

```text
Business Event -> Message
```

An order is created, so an email goes out. A payment succeeds, so another email goes out. Another order, another message.

That works until communication policy gets more sophisticated.

Say a customer places five separate orders in a short window, and each payment is processed independently. A straightforward implementation could send five order confirmations followed by five payment confirmations.

Every event was communicated correctly. The customer experience is still poor.

Maybe the real requirement is to wait for those payments to settle, then send the customer one summary with the orders and their payment statuses.

With a communications domain, the intent can outlive the operation that created it:

```text
OrderCreated #1 ----\
OrderCreated #2 -----\
OrderCreated #3 ------\
OrderCreated #4 -------> Communications
OrderCreated #5 ------/       |
                              | accumulate
                              | correlate
Payment facts --------->      | enrich
                              |
                              v
                     Communication ready
                              |
                              v
                       One summary
```

Ordering doesn't need to know the recipient just placed four other orders. Payment doesn't need to understand the customer's notification experience. Each domain keeps owning its own business behavior.

Communications owns the correlation of intents and the policy that decides when a communication is ready to be composed and delivered.

This is where communication intent differs from a generic event stream. Communications isn't consuming every event in the system and trying to work out which ones deserve a message. The originating services have already stated the intent. Other events and projections may supply information used to fulfill it, but the intent itself is explicit.

That lets communication policy operate over time without turning the business domains into communication orchestrators.

## Some communications are imperative

Not every communication leaves the decisions to Communications.

With an intent like `OrderCreated`, the communications domain decides almost everything: who receives it, which channels to use, whether to send now or wait, even whether to send anything at all. The originating domain only says that something worth communicating happened.

Imperative communications flip that. The originating domain has already decided what must happen, often down to the recipient, the channel, and sometimes the provider.

Take an escheatment notice. Before a business turns unclaimed property over to the state, it may be required to notify the owner by certified mail at their last known address. The recipient is fixed. The channel is fixed. Email isn't an acceptable substitute, and neither is the customer's stated preference for SMS. The provider may be fixed too, if only one vendor can produce the proof of certified delivery the business needs to keep.

A one-time passcode is a smaller example. Identity needs the code delivered by SMS to the phone number registered on the account. Not email because the customer prefers it. Not push. Letting channel policy choose here would undermine the security the code exists to provide.

In both cases, decisions that Communications would normally own are made upstream, because they're part of the business rule itself.

That doesn't move the rest of the work back into the business domain. Identity shouldn't be retrying SMS sends or tracking delivery receipts, and whoever owns escheatment shouldn't be integrating directly with a certified mail vendor. Communications still owns fulfillment: rendering the approved template, attempting delivery, retrying, escalating when delivery fails, and keeping the audit history and evidence of delivery that a regulator or security review may ask for.

So the boundary shifts, but it doesn't disappear. An intent hands Communications a purpose and lets it decide how to fulfill it. An imperative hands it a purpose along with constraints it has to honor. Either way, the business domain decides what must happen, and Communications is accountable for making it happen.

## Templates belong with the communication model

With composition living in Communications, templates get easier to reason about too.

An `OrderCreated` communication may eventually have an email subject, an HTML body, a plain-text body, an SMS variation, a push-notification title, and localized versions of each.

The organization might keep those templates in source control, give Product or Marketing tooling to maintain them, or use a content-management system. That choice can change without touching who owns the underlying order.

Changing:

> Thanks for your order!

to:

> Thanks, Alice! We've received order #123.

is a communication change. So is adding an SMS version, and so is localizing the message. Moving from SMTP to SendGrid is a delivery change.

None of them touch the Ordering model.

That's the practical value of keeping composition inside Communications. Content can evolve with customer-experience requirements while the originating business domains stay focused on their own models.

## The transport is only one part of the problem

When communications first get complicated, provider abstraction tends to get most of the attention. Swapping SMTP for SendGrid, or one SMS provider for another, is an obvious dependency to isolate.

Provider independence is useful. It's also near the bottom of the communications problem.

As the system matures, the harder questions are about policy and composition. Why does this communication exist? Who should receive it, and what information does composing it require? Should it go out now, or wait and be combined with other intents? Which channels and preferences apply? What happens when delivery fails?

Those questions belong together. They describe the lifecycle of communicating with a recipient.

The resulting split looks like this:

```text
Business Domains
      |
      | communication intents
      | communication imperatives
      v
Communications
      |
      | resolve
      | enrich
      | accumulate
      | compose
      | apply policy
      | deliver
      v
Email / SMS / Push / ...
```

The business domains stay responsible for the facts and rules they own. Communications is responsible for turning explicit communication requirements into messages that are composed and delivered correctly.

## Applying the model to eShop

eShop's real service boundaries make it a good place to try this.

When Ordering completes an operation like creating an order, it stays the authority on that business action. As part of completing it, Ordering can express an `OrderCreated` communication intent using the identifiers and facts it owns.

The rest of the lifecycle moves to Communications:

```text
                 Ordering
                    |
              Order created
                    |
                    v
             OrderCreated
                 intent
                    |
                    v
              Communications
                    |
             resolve recipient
                    |
             gather/enrich data
                    |
              compose content
                    |
              apply policy
                    |
             +------+------+
             |      |      |
           Email   SMS    Push
```

Integration events still matter here. They carry facts between bounded contexts and can give Communications what it needs to enrich or complete an existing intent. What they don't need to be is the way Communications discovers that a communication should exist.

The business service can state that decision directly.

Business intent stays explicit where the business operation happens, and communication policy stays in one place, the domain responsible for communication.

## Where Transmitly enters the picture

Everything so far is an architecture problem, not a Transmitly problem.

The point isn't to pick a library and build an architecture around it. The point is to find a boundary that pays off once communications start spanning domains, channels, providers, and time.

Ordering expresses something like `OrderCreated` using information it legitimately owns. Communications accepts that intent and owns its lifecycle from there. Recipient resolution, composition, templates, channel selection, providers, and delivery all stay on the communications side of the boundary.

Transmitly is one way to implement that model.

In the next article we'll add Transmitly to the .NET eShop codebase and put that boundary around an order communication. We'll start with simulated delivery so SMTP credentials and provider configuration don't distract from the architecture.

```text
Ordering
    |
    | OrderCreated
    v
Communications
```

Ordering stays responsible for knowing that an order was created and that the operation gives it a reason to communicate. After that, the communication belongs to a domain built to compose and deliver it.

---

**Next: [Part 2, Adding Email Without Coupling to Email](../part%202/article.md).** Ordering dispatches its first `OrderCreated` intent, and a new Communications service turns it into an email without Ordering ever choosing email.
