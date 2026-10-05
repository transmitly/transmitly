Part 4 ended on one word in the Communications logs:

```text
[Email:Simulation.Email:Dispatched] ...
[Sms:Simulation.Sms:Dispatched] ...
[Push:Simulation.Push:Dispatched] ...
```

`Dispatched` means a provider accepted the message. It doesn't mean anyone received it.

With the simulator the two happen together. With a real provider they don't. Twilio accepts an SMS and returns a message id straight away, then reports back as the message moves along: queued, sent, delivered, or undelivered because the handset was unreachable. Email providers report bounces and opens. Push services report invalid tokens.

Those updates are worth keeping. Support wants to know whether the customer really got the order confirmation. Engineering wants to know which provider is failing. And customers like being able to see what a business has sent them.

This article captures those delivery events in Communications, stores them, and puts them in front of the buyer on a new Messages page.

## Ordering still doesn't change

As in Part 4, nothing in this article touches Ordering. It dispatches `OrderCreated` exactly as it did in Part 3. Delivery events describe what happened to a communication after Ordering stated its intent, and that's Communications' business.

## One message per buyer

Part 4 used the any-match strategy so we could watch all three channels go out at once. For an order confirmation, that's three messages where one would do. Part 5 goes back to Transmitly's default, first match: each buyer gets one message, on the first channel in the pipeline that can reach them.

```text
OrderCreated
    |
    +-- Email  verified email address?  deliver and stop
    |
    +-- SMS    verified phone number?   deliver and stop
    |
    +-- Push   registered device?       deliver
```

Which channel that is depends on the buyer. Communications now only uses addresses the buyer has verified. [The resolver](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/IdentityServerCustomerIdentityResolver.cs) drops the rest before any channel sees them:

```csharp
var addresses = profile.Addresses
    .Where(address => address.IsVerified)
    .Select(address => ...);
```

eShop's two [seeded buyers](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Identity.API/UsersSeed.cs) are set up to show the difference. Alice has verified her email address but not her phone number. Bob has verified his phone number but not his email address. (The seed only creates users that don't exist yet. If your `identitydb` was seeded by an earlier part, the [Part 5 README](README.md) has the SQL to update bob.)

```text
                         Ordering
                            |
           OrderCreated + buyer reference
                            |
               +------------+------------+
               |                         |
             alice                      bob
               |                         |
    resolves to a verified     resolves to a verified
        email address               phone number
               |                         |
               v                         v
             Email                      SMS
```

Ordering sent exactly the same intent for both. It never knew an email address or a phone number existed. Communications resolved each buyer and picked the channel.

In Part 2, Ordering handed over an email address, so every order confirmation was an email. That coupling is gone now, and the buyer's verified addresses decide.

## What a delivery report is

Transmitly represents every delivery event the same way, whatever provider it came from:

```csharp
public record DeliveryReport(
    string EventName,
    string? ChannelId,
    string? ChannelProviderId,
    string? PipelineIntent,
    string? PipelineId,
    string? ResourceId,
    CommunicationsStatus Status,
    object? ChannelCommunication,
    IContentModel? ContentModel,
    Exception? Exception);
```

A few of those fields do most of the work:

```text
EventName           what happened: OnDispatched, OnStatusChanged, OnDelivered, OnError
ChannelId           Email, Sms, Push
ChannelProviderId   who reported it: Simulation.Sms, Twilio, ...
ResourceId          the provider's id for the message
Status              a provider-agnostic outcome, with a success or failure range
```

That's half of what this article is about. Code that records, displays, or alerts on delivery events can be written once, against `DeliveryReport`, and work for every provider.

## Reports come from two places

Some reports are raised at dispatch. When a provider accepts a message, its Transmitly integration raises an `OnDispatched` report with the provider's message id. The simulator does the same, which is what produced those log lines.

Others arrive later. A provider like Twilio calls a webhook when a message's status changes, and Transmitly turns that callback into an `OnStatusChanged` report.

```text
Dispatch
   |
   v
Provider accepts the message ------> OnDispatched report
   |
   | seconds or minutes later
   v
Provider calls our webhook --------> OnStatusChanged report
```

Both kinds reach the same place. Next to the logging handler from Part 2, Communications [registers a delivery report handler](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/Program.cs) for recording:

```csharp
.AddDeliveryReportHandler(report =>
{
    deliveryReports.Enqueue(report);
    return Task.CompletedTask;
})
```

It doesn't check which provider sent the report, or whether the report came from a dispatch or a webhook. It just queues it.

## Recording reports in the background

The handler only queues because of where reports are raised: in the middle of a dispatch, or in the middle of a webhook request from a provider. Neither is a good place to wait on a database.

A [background service](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/DeliveryReports/DeliveryReportProcessing.cs) reads the queue and records one report at a time:

```csharp
await foreach (var report in queue.ReadAllAsync(stoppingToken))
{
    await using var scope = scopeFactory.CreateAsyncScope();
    var recorder = scope.ServiceProvider.GetRequiredService<DeliveryReportRecorder>();
    await recorder.RecordAsync(report, stoppingToken);
}
```

Processing reports in order also means a quick status update can't race the dispatch report it follows. A report that fails to record is logged, and the processor moves on to the next one.

## Communications gets a database

Delivery history is communications data, so it goes in a database Communications owns. The [AppHost](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/eShop.AppHost/Program.cs) adds `communicationsdb` to eShop's existing Postgres server, next to the Catalog and Ordering databases:

```csharp
var communicationsDb = postgres.AddDatabase("communicationsdb");
```

The model has [two tables](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/DeliveryReports/CommunicationRecord.cs). A communication is one message to one recipient on one channel, with its latest status. A delivery event is one report about it.

```text
Communications
    Id
    PipelineIntent      OrderCreated
    ChannelId           Sms
    ChannelProviderId   Twilio
    ResourceId          SM8f2c...     the provider's message id
    RecipientId         the buyer's identity
    Summary             "We've received your eShop order #123. ..."
    Status              Delivered
    IsFailure           false

DeliveryEvents
    CommunicationRecordId
    EventName           OnStatusChanged
    Status              Delivered
    ProviderDetails     {"MessageSid": "SM8f2c...", "ErrorCode": null, ...}
    ReceivedAt
```

The [recorder](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/DeliveryReports/DeliveryReportRecorder.cs) ties reports together through the provider's message id. The first report for a message creates its communication. Later reports with the same channel and message id add events to it:

```csharp
var record = await FindAsync(report, cancellationToken);

if (record is null)
{
    record = new CommunicationRecord { /* intent, channel, provider, message id ... */ };
    db.Communications.Add(record);
}

// A provider's later update carries less context than the dispatch, so only fill gaps.
record.RecipientId ??= DeliveryReportDetails.RecipientId(report);
record.Summary ??= DeliveryReportDetails.Summary(report.ChannelCommunication);

record.Status = report.Status.Type;
record.IsFailure = report.Status.IsFailure();
```

The report raised at dispatch knows a lot: the rendered message, the content model, and the recipient profile inside it. A webhook from Twilio knows only its message id and the new status. Filling gaps keeps the richer details from the dispatch.

The latest report wins. Providers don't always report in order, and a production system might want to make sure a late `sent` never overwrites `delivered`. For the sample, the simple rule is enough.

## Receiving provider webhooks

Transmitly's ASP.NET Core package, `Transmitly.Microsoft.AspnetCore.Mvc`, supplies a controller for delivery report webhooks. Communications [exposes it](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/DeliveryReports/DeliveryReportsController.cs) at one address for every provider:

```csharp
[AllowAnonymous]
[Route("api/communications/delivery-reports")]
public sealed class DeliveryReportsController(ICommunicationsClient communicationsClient)
    : ChannelProviderDeliveryReportController(communicationsClient);
```

and [registers the model binders](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/Program.cs) that do the actual work:

```csharp
builder.Services.AddControllers(options => options.AddTransmitlyDeliveryReportModelBinders());
```

When a request arrives, the binder offers it to each registered provider's request adaptor. Each adaptor recognizes its own provider's requests and turns them into `DeliveryReport`s, which the controller hands back to Transmitly. From there they go to the same handlers as every other report.

```text
POST /api/communications/delivery-reports?...
        |
        v
Delivery report model binder
        |
        +-- Twilio SMS adaptor     "this is mine" -> DeliveryReport
        +-- Twilio voice adaptor   not mine
        +-- ...
        |
        v
Transmitly delivery report handlers
        |
        v
DeliveryReportQueue
```

The webhook is anonymous because providers can't sign in. A production deployment should validate each provider's request signature before trusting a callback. Twilio's adaptor captures the signature header, but the sample doesn't check it.

## Using Twilio as the example

The sample uses Twilio for this part because it's a provider that really does report delivery status after dispatch. It's [wired up](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/EshopChannelProviders.cs), but only switched on when credentials are configured:

```csharp
if (_twilio is null)
{
    builder.AddSimulationSupport(providerId: "Sms");
}
else
{
    builder.AddTwilioSupport(twilio =>
    {
        twilio.AccountSid = _twilio.AccountSid;
        twilio.AuthToken = _twilio.AuthToken;
    });
}
```

Without Twilio settings, SMS stays on the simulator and everything in this article still works. The simulator raises its `OnDispatched` reports, the recorder stores them, and the Messages page shows them.

With Twilio configured, Twilio also needs to know where to send status updates. [The SMS channel](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/Program.cs) tells it, using Twilio's own channel settings:

```csharp
sms.Twilio().StatusCallbackUrl = providers.DeliveryReportUrl;
```

When it sends each message, Transmitly gives Twilio the webhook address plus a few query parameters that identify the intent, the channel, and the provider:

```text
https://<public host>/api/communications/delivery-reports
    ?tlyp=OrderCreated
    &tlyc=Sms
    &tlycp=Twilio
```

Those parameters are how Twilio's adaptor recognizes the callback as its own when it comes back. The address has to be reachable from Twilio's servers, so local development needs a tunnel.

The configuration goes in the Communications API's user secrets or environment variables, and looks like this:

```json
{
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "...",
    "FromNumber": "+1..."
  },
  "Communications": {
    "DeliveryReportUrl": "https://<public host>/api/communications/delivery-reports"
  }
}
```

## Provider details when you need them

The other half of this article is the provider's own detail. A provider-agnostic status is right for almost everything, but sometimes you need to know exactly what the provider said.

When Twilio reports an undelivered SMS, the shared status says the message failed. Twilio's error code says why: `30003` means the handset was unreachable, `30005` means the number doesn't exist, and `30007` means a carrier filtered the message. Each of those calls for a different response.

Transmitly keeps the provider's details on the report, and each provider package exposes them through its own extension. The recorder [stores Twilio's](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/DeliveryReports/DeliveryReportDetails.cs) alongside the shared status:

```csharp
if (report.ChannelProviderId?.StartsWith(Id.ChannelProvider.Twilio(), StringComparison.OrdinalIgnoreCase) == true
    && report.ChannelId == Id.Channel.Sms())
{
    var sms = report.Twilio().Sms;
    return JsonSerializer.Serialize(new
    {
        sms.MessageSid,
        MessageStatus = sms.MessageStatus?.ToString(),
        sms.ErrorCode,
        sms.To,
        sms.From
    });
}
```

That's the only provider-specific code in the recording path, and it's optional. Remove it and every Twilio report is still recorded with its status. Keep it and the details are there for the support engineer who needs them.

## The buyer's inbox

With communications stored, the buyer can see them. Communications exposes an [inbox](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/Apis/InboxApi.cs) for the signed-in user:

```csharp
endpoints.MapGet("/api/communications/inbox", GetInboxAsync)
    .RequireAuthorization();
```

It returns the user's 50 most recent communications, newest first, from the records the delivery reports built. The inbox matches records against the `sub` claim on the user's token, which is the same identity id Ordering dispatched in Part 3, so no new mapping is needed.

Authentication follows eShop's existing pattern. Identity gets a [`communications` scope](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Identity.API/Configuration/Config.cs), the web app requests it when the user signs in, and Communications validates tokens with eShop's shared `AddDefaultAuthentication()`. The [web app's HTTP client](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/WebApp/Extensions/Extensions.cs) for Communications attaches the user's token:

```csharp
builder.Services.AddHttpClient<InboxService>(o => o.BaseAddress = new("https+http://communications-api"))
    .AddAuthToken();
```

The web app adds a [Messages page](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/WebApp/Components/Pages/User/Messages.razor) to the [account menu](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/WebApp/Components/Layout/UserMenu.razor), next to My orders. Here's what alice and bob each see after placing an order:

```text
alice
Sent                Channel   Message                                    Status
10/2/2026 8:17 PM   Email     Thanks for your order #11                  Dispatched
                              Order created

bob
Sent                Channel   Message                                    Status
10/2/2026 8:18 PM   SMS       We've received your eShop order #12. ...   Dispatched
                              Order created
```

Each of them placed one order and got one message, on different channels, because their verified addresses differ. With Twilio configured, bob's status starts as whatever Twilio returned when it accepted the message (usually queued) and changes as Twilio's callbacks arrive.

The same records could feed other things too. A support tool could show a customer's full delivery history, or an alert could fire when one provider's failure rate climbs. Neither needs anything new from the providers, or from Ordering.

## Testing without a provider

None of this needs a real Twilio account to test. [The sample's tests](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/tests/Application.UnitTests/DeliveryReportWebhookTests.cs) post a Twilio-shaped status callback to the real `DeliveryReportsController`, running in an in-memory test server, at the URL Twilio would be given:

```csharp
var callbackUrl = new Uri("http://localhost/api/communications/delivery-reports")
    .AddPipelineContext(string.Empty, CommunicationIntents.OrderCreated, null, Id.Channel.Sms(), Id.ChannelProvider.Twilio());

await client.PostAsync(callbackUrl, new FormUrlEncodedContent(new Dictionary<string, string>
{
    ["SmsSid"] = "SM123",
    ["MessageSid"] = "SM123",
    ["SmsStatus"] = "delivered",
    ["MessageStatus"] = "delivered",
    ["AccountSid"] = "ACtest"
}));
```

The callback comes out the other side as an `OnStatusChanged` report for `SM123`, with a `Delivered` status. [Another test](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/tests/Application.UnitTests/DeliveryReportRecordingTests.cs) records a dispatch followed by an undelivered callback, and checks that both land on the same communication with Twilio's error code kept.

## Where we ended up

Communications now keeps a record of everything it sends, and of everything providers tell it afterwards:

```text
Ordering
    |
    | OrderCreated
    v
Communications
    |
    +-- compose and dispatch          (Parts 2-4)
    |
    +-- delivery reports
    |       at dispatch               simulator, Twilio, ...
    |       from webhooks             Twilio status callbacks
    |
    +-- one handler -> queue -> recorder -> communicationsdb
    |
    v
Inbox API -> WebApp Messages page
```

Because `DeliveryReport` is provider-agnostic, the recording, the inbox, and anything else built on delivery history work the same for every provider. When one provider's specifics matter, its details are still there.

Adding a provider doesn't change any of it. A new provider's package brings its own request adaptor, and its callbacks start landing in the same tables.

## Looking back

This series started with a line most applications have somewhere:

```csharp
await emailClient.SendAsync(...);
```

The usual next step is to inject that client into the service that knows something happened. That holds up for a while. Then the service learns how to find the customer's email address, then what to put in the message, then which template to use. When SMS arrives, it learns about phone numbers and a second provider SDK. When someone asks whether the message actually arrived, it learns about webhooks. Each step is reasonable. Together they turn a business service into a communications service that happens to manage orders.

eShop never went down that road. Ordering states one intent when an order is created, names the buyer by identity, and hands over the order facts it already has. Its code last changed in Part 3, when it started sending an identity reference and the order's items. Everything after that (two more channels, a different channel for each buyer, delivery tracking from any provider, and an inbox) happened without touching Ordering.

The first thing Transmitly gave us was a stable seam. Application code talks to `ICommunicationsClient` and an intent name, and that's all. Whether the pipeline runs in the same process or behind a Communications service is a middleware decision, and so is waiting for a database commit before anything goes out. Ordering didn't have to know about either.

Behind that seam, each concern could live with the data it needs. Recipients are resolved through Identity, which owns them, instead of being copied into every service that sends something. Product details come from Catalog at composition time rather than being dragged through Ordering. And because a content model built for communicating sits between the business facts and the templates, email, SMS, and push each use it differently without anyone reassembling the data.

Delivery decisions moved out of business code. A buyer's verified addresses decide which channels can reach them, the pipeline's delivery strategy decides whether they get one message or several, and a provider registration decides which company carries each channel. Swapping SMTP for SendGrid, or the simulator for Twilio, leaves the pipeline and every dispatching service alone.

We also got to see what happened after the send. Reports from the simulator and from Twilio arrive in one provider-agnostic shape, so the recording and the inbox were written once, and provider details like Twilio's error code are still available when they matter.

None of it needed a provider account to build. The simulator ran the whole pipeline, and the tests composed real messages through the real templates and enrichers without touching the network.

## Shaped around eShop

None of that required eShop to reorganize itself around a library. Look at the pieces we wrote:

```text
IdentityServerCustomerIdentityResolver   IPlatformIdentityResolver
PushRegistrationProfileEnricher          IPlatformIdentityProfileEnricher
CatalogContentModelEnricher              IContentModelEnricher
EshopCommunicationsMiddleware            ICommunicationClientMiddleware
TransactionalCommunicationsMiddleware    ICommunicationClientMiddleware
```

Each one adapts Transmitly to something eShop already had. [Buyers stayed in Identity](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/IdentityServerCustomerIdentityResolver.cs), behind an endpoint Identity owns and a service token Identity issues. [Product data](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Communications.API/OrderCreated/CatalogContentModelEnricher.cs) stayed behind Catalog's existing batch endpoint. [The commit deferral](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Ordering.API/Infrastructure/Communications/TransactionalCommunicationsMiddleware.cs) hooks into Ordering's own [`TransactionBehavior`](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/Ordering.API/Application/Behaviors/TransactionBehavior.cs). [Forwarding](https://github.com/transmitly/transmitly/blob/main/samples/eShop%20Series/part%205/src/eShop.ServiceDefaults/Communications/EshopCommunicationsMiddleware.cs) uses Aspire's service discovery and eShop's shared service defaults. Transmitly supplied the extension points, and eShop decided what went in them.

That's the right way round. A communications library that dictates where identities live, how transactions work, or how services talk to each other forces its way of working onto every system it touches. Transmitly asks for an implementation of a small interface at the point it needs one.

The built-in pieces work the same way, so when something isn't in the box, it can be added without changing Transmitly:

- A channel provider is a dispatcher registered against the channels it supports. A provider Transmitly doesn't ship, an in-house SMS gateway for example, is an `IChannelProviderDispatcher<T>` and a registration.
- Provider webhooks come in through `IChannelProviderDeliveryReportRequestAdaptor`, the same interface Twilio's adaptor implements in this article.
- Template engines implement `ITemplateEngine`, which is how the Fluid and Scriban integrations work.
- Delivery strategies derive from `BasePipelineDeliveryStrategyProvider`, for when first match and any match don't describe your policy.
- Channels implement `IChannel`, for a medium beyond email, SMS, push, and voice.

The packaged providers and integrations are small, separate repositories, which makes them good references for writing your own.

## There's a lot more in Transmitly

The series used a fraction of what Transmitly does. A few of the pieces we didn't get to:

- Our templates were C# methods that built strings. Transmitly has pluggable template engines, with Fluid and Scriban integrations, so content can live in templates instead of code.
- Beyond SMTP, Twilio, and Firebase there are SendGrid and Mailgun for email, and Infobip for email, SMS, and voice. Voice is a channel in its own right, supported by Twilio and Infobip.
- Personas define audience segments as conditions over the recipient profile with `AddPersona`, and a pipeline can apply only to the segments it names with `AddPersonaFilter`. They're a natural next step from this article's verified addresses.
- A dispatch can name the channels a recipient has chosen, and Transmitly will only deliver on those. Part 2's contract already carries them as `AllowedChannels`.
- We used client middleware to forward dispatches and to wait for a commit. Dispatch middleware runs around each channel dispatch inside the pipeline, which makes it a good home for concerns like auditing.
- Delivery report handlers can filter on specific events, channels, providers, or intents, instead of receiving every report like ours did.

The best place to go from here is the repository: [github.com/transmitly/transmitly](https://github.com/transmitly/transmitly). It has the list of channel providers and integrations, the [samples](https://github.com/transmitly/transmitly/tree/main/samples), including this series and smaller starting points like Hello Transmitly and the Microservices sample, and the [wiki](https://github.com/transmitly/transmitly/wiki).

If you try it and something is confusing, missing, or more ceremony than it's worth, I want to hear about it in [GitHub Discussions](https://github.com/transmitly/transmitly/discussions).
