Part 4 ended on one word in the Communications logs:

```text
[Email:Simulation.Email:Dispatched] ...
[Sms:Simulation.Sms:Dispatched] ...
[Push:Simulation.Push:Dispatched] ...
```

`Dispatched` means a provider accepted the message. It doesn't mean anyone received it.

With the simulator, those are the same moment. Real providers rarely work that way. Twilio accepts an SMS and returns a message id straight away, then reports back as the message moves along: queued, sent, delivered, or undelivered because the handset was unreachable. Email providers report bounces and opens. Push services report invalid tokens.

Those updates are worth keeping. Support wants to know whether the customer really got the order confirmation. Engineering wants to know which provider is failing. And customers like being able to see what a business has sent them.

This article captures those delivery events in Communications, stores them, and puts them in front of the buyer on a new Messages page.

## Ordering still doesn't change

Same as Part 4: nothing in this article touches Ordering. It dispatches `OrderCreated` exactly as it did in Part 3. Delivery events describe what happened to a communication after Ordering stated its intent, and that's Communications' business.

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

Which channel that is depends on the buyer, and that's where identity resolution earns its keep. Communications now only uses addresses the buyer has verified. The resolver drops the rest before any channel sees them:

```csharp
var addresses = profile.Addresses
    .Where(address => address.IsVerified)
    .Select(address => ...);
```

eShop's two seeded buyers are set up to show the difference. Alice has verified her email address but not her phone number. Bob has verified his phone number but not his email address.

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

Ordering sent exactly the same intent for both. It never knew an email address or a phone number existed. Communications resolved each buyer, applied its policy, and picked the channel.

That's the coupling Part 2 started with, removed. Back then Ordering handed over an email address, so every order confirmation was an email. Now the buyer's identity decides.

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
EventName           what happened: dispatched, status changed, delivered, error
ChannelId           Email, Sms, Push
ChannelProviderId   who reported it: Simulation.Sms, Twilio, ...
ResourceId          the provider's id for the message
Status              a provider-agnostic outcome, with a success or failure range
```

That's the first half of this article's point. Code that records, displays, or alerts on delivery events can be written once, against `DeliveryReport`, and work for every provider.

## Reports come from two places

Some reports are raised at dispatch. When a provider accepts a message, its Transmitly integration raises a `Dispatched` report with the provider's message id. The simulator does the same, which is what produced those log lines.

Others arrive later. A provider like Twilio calls a webhook when a message's status changes, and Transmitly turns that callback into another `DeliveryReport`.

```text
Dispatch
   |
   v
Provider accepts the message ------> Dispatched report
   |
   | seconds or minutes later
   v
Provider calls our webhook --------> StatusChanged report
```

Both kinds reach the same place. Communications registers one delivery report handler:

```csharp
.AddDeliveryReportHandler(report =>
{
    deliveryReports.Enqueue(report);
    return Task.CompletedTask;
})
```

It doesn't check which provider sent the report, or whether the report came from a dispatch or a webhook. It queues it.

## Recording reports in the background

The handler only queues because of where reports are raised: in the middle of a dispatch, or in the middle of a webhook request from a provider. Neither is a good place to wait on a database.

A background service reads the queue and records one report at a time:

```csharp
await foreach (var report in queue.ReadAllAsync(stoppingToken))
{
    await using var scope = scopeFactory.CreateAsyncScope();
    var recorder = scope.ServiceProvider.GetRequiredService<DeliveryReportRecorder>();
    await recorder.RecordAsync(report, stoppingToken);
}
```

Processing reports in order also means a quick status update can't race the dispatch report it follows.

## Communications gets a database

Delivery history is communications data, so it goes in a database Communications owns. The AppHost adds `communicationsdb` to eShop's existing Postgres server, next to Catalog's and Ordering's:

```csharp
var communicationsDb = postgres.AddDatabase("communicationsdb");
```

The model has two tables. A communication is one message to one recipient on one channel, with its latest status. A delivery event is one report about it.

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

The recorder ties reports together through the provider's message id. The first report for a message creates its communication. Later reports with the same channel and message id add events to it:

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

Now the provider side. Transmitly's ASP.NET Core package, `Transmitly.Microsoft.AspnetCore.Mvc`, supplies a controller for delivery report webhooks. Communications exposes it at one address for every provider:

```csharp
[AllowAnonymous]
[Route("api/communications/delivery-reports")]
public sealed class DeliveryReportsController(ICommunicationsClient communicationsClient)
    : ChannelProviderDeliveryReportController(communicationsClient);
```

and registers the model binders that do the actual work:

```csharp
builder.Services.AddControllers(options => options.AddTransmitlyDeliveryReportModelBinders());
```

When a request arrives, the binder offers it to each registered provider's request adaptor. Each adaptor recognizes its own provider's requests and turns them into `DeliveryReport`s, which the controller hands back to Transmitly. From there they go to the same handler as every other report.

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

The sample uses Twilio for this part, because it's a provider that really does report delivery status after dispatch. It's wired up, but only switched on when credentials are configured:

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

Without Twilio settings, SMS stays on the simulator and everything in this article still works. The simulator raises its `Dispatched` reports, the recorder stores them, and the Messages page shows them.

With Twilio configured, Twilio also needs to know where to send status updates. The SMS channel tells it, using Twilio's own channel settings:

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

The configuration looks like this:

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

Here's the second half of the point. A provider-agnostic status is right for almost everything, but sometimes you need to know exactly what the provider said.

When Twilio reports an undelivered SMS, the shared status says the message failed. Twilio's error code says why: `30003` means the handset was unreachable, `30005` means the number doesn't exist, and `30007` means a carrier filtered the message. Those lead to very different actions.

Transmitly keeps the provider's details on the report, and each provider package exposes them through its own extension. The recorder stores Twilio's alongside the shared status:

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

With communications stored, the buyer can see them. Communications exposes an inbox for the signed-in user:

```csharp
endpoints.MapGet("/api/communications/inbox", GetInboxAsync)
    .RequireAuthorization();
```

It returns the user's most recent communications, newest first, from the records the delivery reports built. The user is identified by the same identity id Ordering dispatched in Part 3, so no new mapping is needed.

Authentication follows eShop's existing pattern. Identity gets a `communications` scope, the web app requests it when the user signs in, and the web app's HTTP client for Communications attaches the user's token:

```csharp
builder.Services.AddHttpClient<InboxService>(o => o.BaseAddress = new("https+http://communications-api"))
    .AddAuthToken();
```

The web app adds a Messages page to the account menu, next to My orders. After placing an order, it looks like this:

```text
Sent                Channel   Message                                    Status
10/2/2026 4:04 PM   Email     Thanks for your order #123                 Dispatched
                              Order created
10/2/2026 4:04 PM   SMS       We've received your eShop order #123. ...  Dispatched
                              Order created
10/2/2026 4:04 PM   Push      Thanks for your order                      Dispatched
                              Order created
```

With Twilio configured, the SMS row moves on from `Dispatched` as Twilio's updates arrive.

The same records could feed other things too. A support tool could show a customer's full delivery history. An alert could fire when one provider's failure rate climbs. A retry policy could resend an undelivered SMS as an email. None of those need anything new from the providers, or from Ordering.

## Testing without a provider

None of this needs a real Twilio account to test. The sample's tests post a Twilio-shaped status callback to the real `DeliveryReportsController`, running in an in-memory test server, at the URL Twilio would be given:

```csharp
var callbackUrl = new Uri("http://localhost/api/communications/delivery-reports")
    .AddPipelineContext(string.Empty, CommunicationIntents.OrderCreated, null, Id.Channel.Sms(), Id.ChannelProvider.Twilio());

await client.PostAsync(callbackUrl, new FormUrlEncodedContent(new Dictionary<string, string>
{
    ["SmsSid"] = "SM123",
    ["MessageSid"] = "SM123",
    ["SmsStatus"] = "delivered",
    ["MessageStatus"] = "delivered"
}));
```

The callback comes out the other side as a `StatusChanged` report for `SM123`, with a `Delivered` status. Another test records a dispatch followed by an undelivered callback, and checks that both land on the same communication with Twilio's error code kept.

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

The provider-agnostic `DeliveryReport` means the recording, the inbox, and anything else built on delivery history work the same for every provider. When one provider's specifics matter, its details are still there.

Adding a provider doesn't change any of it. A new provider's package brings its own request adaptor, and its callbacks start landing in the same tables.

In Part 6 we return to enrichment, the stage that's quietly been doing a lot of the work since Part 3, and see how far it can go.
