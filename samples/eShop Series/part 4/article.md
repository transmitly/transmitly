Part 3 built the content. Ordering dispatches `OrderCreated` with the order facts it owns, Communications resolves the buyer through Identity, a Catalog enricher fills in product details, and one enriched context comes out the other side. Then a single channel used it: email.

Part 3 also sketched what SMS and push could do with the same context. This article builds them. Along the way we'll separate two ideas that are easy to blur together, channels and providers, and see what it would take to connect real ones like SMTP, Twilio, and Firebase.

The code stays on Transmitly's simulator the whole time. Nothing in this article needs an account, an API key, or a phone.

## Ordering doesn't change

Start with what stays the same. Ordering still dispatches exactly what it dispatched in Part 3:

```csharp
await _communicationsClient.DispatchAsync(
    CommunicationIntents.OrderCreated,
    [buyer],
    TransactionModel.Create(model),
    cancellationToken: cancellationToken);
```

There's no `channels` argument, no SMS flag, no push token. Comparing `Ordering.API` between the Part 3 and Part 4 code turns up no differences at all.

That's the payoff Part 1 promised. Ordering said an order was created. Whether that becomes one message or three is a communication decision, and every change in this article happens inside Communications.

## Every channel needs an address

A channel can only deliver if the recipient has an address it understands. Email needs an email address. SMS needs a phone number. Push needs a device token.

Transmitly handles the matching. When it plans a dispatch, it asks each channel in the pipeline which of the recipient's addresses it can use. The email channel accepts addresses typed as email or shaped like one. The SMS channel accepts phone numbers. The push channel accepts device tokens and topics. A channel with no usable address is skipped for that recipient.

So adding channels starts with the recipient profile. Here's what eShop's buyer looks like after identity resolution:

```text
Buyer: alice

Email
    AliceSmith@email.com

Phone
    1234567890
```

Identity already owns the email address and the phone number, and the resolver from Part 3 returns both. SMS has what it needs.

Push doesn't. There's no device token anywhere in eShop, and no service in the application that would own one.

## Push tokens are communications data

A device token isn't really identity data. It doesn't say who the buyer is. It says where a notification can be delivered, it changes whenever the app is reinstalled, and nothing outside communications has any use for it.

That makes it communications data, and Communications should own it.

In Part 3 we noted that the pipeline has a stage for exactly this kind of thing, and that eShop didn't need it yet:

```text
Identity resolution
    |
    v
Identity-profile enrichment     <-- push tokens go here
    |
    v
Content enrichment
```

Profile enrichers run after resolution and before content is built. They can add to the recipient's profile, which is where channels look for addresses. So Communications gets a small store of push registrations, and a profile enricher that reads from it:

```csharp
public sealed class PushRegistrationProfileEnricher(IPushRegistrationStore registrations)
    : IPlatformIdentityProfileEnricher
{
    public async Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
    {
        if (identityProfile is not CustomerIdentityProfile customer)
        {
            return;
        }

        foreach (var token in await registrations.GetDeviceTokensAsync(customer.Id))
        {
            customer.AddAddress(new PlatformIdentityAddress(
                token,
                type: PlatformIdentityAddress.Types.DeviceToken()));
        }
    }
}
```

The enricher is registered for the same identity type as the resolver:

```csharp
.AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
.AddPlatformIdentityProfileEnricher<PushRegistrationProfileEnricher>(CommunicationIdentityTypes.Buyer)
```

One gap remains. eShop's apps don't register for push notifications, so there's nothing to put in the store. The sample registers a `SimulatedPushRegistrationStore` that gives every buyer one simulated device:

```csharp
public Task<IReadOnlyCollection<string>> GetDeviceTokensAsync(string identityId, CancellationToken cancellationToken = default) =>
    Task.FromResult<IReadOnlyCollection<string>>([$"simulated-device-{identityId}"]);
```

In a real system, the mobile app would send its token to Communications when the user signs in, and the store would hold whatever devices are registered. The enricher wouldn't change.

After enrichment the profile has an address for every channel:

```text
Buyer: alice

Email
    AliceSmith@email.com

Phone
    1234567890

Device token
    simulated-device-…
```

## One context, three renderings

With addresses in place, the pipeline adds two channels next to the email:

```csharp
.AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
{
    pipeline.UseAnyMatchPipelineDeliveryStrategy();

    pipeline.AddEmail(
        "orders@eshop.local".AsIdentityAddress("eShop"),
        email =>
        {
            email.AddChannelProviderFilter(EshopChannelProviders.Email);
            email.Subject.AddTemplateResolver(context =>
                Task.FromResult<string?>(OrderCreatedEmail.Subject(context)));
            email.TextBody.AddTemplateResolver(context =>
                Task.FromResult<string?>(OrderCreatedEmail.TextBody(context, webAppUrl)));
        });

    pipeline.AddSms(
        "+15555550100".AsIdentityAddress("eShop"),
        sms =>
        {
            sms.AddChannelProviderFilter(EshopChannelProviders.Sms);
            sms.Message.AddTemplateResolver(context =>
                Task.FromResult<string?>(OrderCreatedSms.Message(context, webAppUrl)));
        });

    pipeline.AddPushNotification(push =>
    {
        push.AddChannelProviderFilter(EshopChannelProviders.Push);
        push.Title.AddTemplateResolver(context =>
            Task.FromResult<string?>(OrderCreatedPush.Title(context)));
        push.Body.AddTemplateResolver(context =>
            Task.FromResult<string?>(OrderCreatedPush.Body(context)));
        push.AddData(OrderCreatedPush.ActionKey, OrderCreatedPush.OpenOrderAction);
        push.AddDataIfNotNull(OrderCreatedPush.OrderIdKey, context =>
            Task.FromResult(OrderCreatedPush.OrderId(context)));
    });
});
```

We'll come back to the first line and the provider filters. For now, look at what each channel renders.

All three read the same `OrderCreatedContentModel` that Part 3's Catalog enricher built. That enricher is registered per recipient, not per channel, so it runs once and every channel shares its result. Adding SMS and push didn't add any calls to Catalog.

The email is unchanged from Part 3. The SMS keeps only what fits in a text message:

```csharp
public static string Message(IDispatchCommunicationContext context, Uri? webAppUrl)
{
    var order = OrderCreatedContentModel.From(context.ContentModel);
    var message = order is null
        ? "We've received your eShop order."
        : $"We've received your eShop order #{order.OrderId}.";

    return OrderLinks.Orders(webAppUrl) is { } ordersLink
        ? $"{message}\nView your order: {ordersLink}"
        : message;
}
```

which renders as:

```text
We've received your eShop order #123.
View your order: https://eshop.example/user/orders
```

Push is shorter still. The visible part is a title and a sentence. The useful part is the data: an action and an order id the app can use to open the right screen.

```text
Title
    Thanks for your order

Body
    Order #123 has been received.

Data
    action:  open-order
    orderId: 123
```

Three channels, three very different messages, one content model. None of the templates had to fetch anything.

## Deciding which channels to use

Now the first line of the pipeline:

```csharp
pipeline.UseAnyMatchPipelineDeliveryStrategy();
```

A pipeline with several channels needs a rule for using them, and it helps to know what happens without that line first.

By default, Transmitly walks the channels in the order they were added and delivers on the first one that can reach the recipient. If delivery on that channel fails, it moves on to the next. For our pipeline, that means the buyer gets the email, and SMS and push only come into play if the email can't be delivered:

```text
Default (first match)

OrderCreated
    |
    +-- Email  reachable? deliver and stop
    |
    +-- SMS    otherwise, try this
    |
    +-- Push   last resort
```

One message per order, with a fallback built in. For an order confirmation, that's usually the behavior you want. Reordering the channels changes the ladder: put push first and app users get a notification, while everyone else falls through to email.

Transmitly calls this rule a delivery strategy, and a pipeline can pick a different one. This sample opts into any match, which sends on every channel the recipient can be reached on, so we can watch all three channels go out:

```text
Any match

OrderCreated
    |
    +-- Email  -> delivered
    +-- SMS    -> delivered
    +-- Push   -> delivered
```

Either way, the choice is one line in Communications, and Ordering doesn't know which one is in use. A future policy could go further and respect each recipient's channel preferences. Part 2's dispatch contract already carries `AllowedChannels` for that.

## Channels and providers are different things

So far "SMS" has meant two things at once: the kind of message, and whoever delivers it. Transmitly keeps those apart.

A **channel** is the medium: email, SMS, push. Channels own content. They know a subject line from a message body, and what an address has to look like.

A **channel provider** is who actually delivers it: an SMTP server, SendGrid, Twilio, Firebase. Providers own transport. They know about API keys, endpoints, and rate limits.

```text
          Channels                    Providers
     (what we send)              (who delivers it)

   Email  ---------------------->  SMTP / SendGrid
   SMS    ---------------------->  Twilio
   Push   ---------------------->  Firebase
```

The pipeline is written entirely in terms of channels. Providers are registered separately, once, and the two meet only at dispatch time.

In this sample, all three providers are simulators:

```csharp
public static class EshopChannelProviders
{
    public static string Email { get; } = Id.ChannelProvider.Simulation("Email");
    public static string Sms { get; } = Id.ChannelProvider.Simulation("Sms");
    public static string Push { get; } = Id.ChannelProvider.Simulation("Push");

    public static CommunicationsClientBuilder AddEshopChannelProviders(this CommunicationsClientBuilder builder) =>
        builder
            .AddSimulationSupport(providerId: "Email")
            .AddSimulationSupport(providerId: "Sms")
            .AddSimulationSupport(providerId: "Push");
}
```

Each simulator stands in for the real provider that would sit behind its channel. That's what the `AddChannelProviderFilter` calls in the pipeline are for: each channel names the one provider it should use.

Real providers declare the channels they support, so a Twilio registration would never be offered an email. A simulator will happily accept any channel, so naming each channel's provider keeps the routing explicit and keeps the simulated setup shaped like the real one.

## Connecting real providers

Here's what changes to send real messages. The pipeline doesn't, and neither do the templates, the enrichers, or anything outside Communications.

Each provider is a package:

```text
Transmitly.ChannelProvider.Smtp
Transmitly.ChannelProvider.Twilio
Transmitly.ChannelProvider.Firebase
```

and each replaces one simulator registration, with its credentials coming from configuration:

```csharp
public static CommunicationsClientBuilder AddEshopChannelProviders(
    this CommunicationsClientBuilder builder,
    IConfiguration configuration) =>
    builder
        .AddSmtpSupport(smtp =>
        {
            smtp.Host = configuration["Smtp:Host"];
            smtp.Port = 587;
            smtp.UserName = configuration["Smtp:UserName"];
            smtp.Password = configuration["Smtp:Password"];
        })
        .AddTwilioSupport(twilio =>
        {
            twilio.AccountSid = configuration["Twilio:AccountSid"];
            twilio.AuthToken = configuration["Twilio:AuthToken"];
        })
        .AddFirebaseSupport(firebase =>
        {
            firebase.ProjectId = configuration["Firebase:ProjectId"];
            firebase.Credential = FirebaseCredential.GetApplicationDefault();
        });
```

The provider ids the channels pin to change with them:

```csharp
public static string Email { get; } = Id.ChannelProvider.Smtp();
public static string Sms { get; } = Id.ChannelProvider.Twilio();
public static string Push { get; } = Id.ChannelProvider.Firebase();
```

That's the whole code change. The rest is operational: an SMTP server or a SendGrid account, a Twilio number to replace the placeholder `+15555550100` the SMS is sent from, a Firebase project, and a push registration store that real devices write to.

Swapping one provider for another works the same way. Moving email from SMTP to SendGrid replaces `AddSmtpSupport` with `AddSendGridSupport` and changes one id.

Providers can also be combined. With first-match delivery, a channel that has two providers gets a fallback for free: if the first provider fails, Transmitly tries the next before moving on to the next channel. A team could run SendGrid for email and keep SMTP behind it for the day SendGrid has an outage.

None of these choices reach the services that dispatch intents. Ordering didn't change when we added channels, and it wouldn't change if every provider changed tomorrow.

## Why stay on the simulator?

It's tempting to wire up a real provider as soon as possible. There's a good case for not doing it in the sample, and for keeping the simulator around even after real providers exist.

The simulator runs the whole pipeline. Identity resolution, both enrichers, every template, and the channel and provider selection all happen exactly as they would in production. Only the last step, handing the message to an external service, is simulated.

It also reports what it would have sent. Every simulated delivery raises a delivery report with the channel, the provider, and the full rendered communication. The Communications service logs those reports, so placing an order in the web app produces three entries in the `communications-api` logs in the Aspire dashboard:

```text
[Email:Simulation.Email:Dispatched] ...
[Sms:Simulation.Sms:Dispatched] ...
[Push:Simulation.Push:Dispatched] ...
```

each followed by the rendered message.

And it makes the composition testable. The sample's tests build the real `OrderCreated` pipeline with fake Identity and Catalog services, dispatch an order, and check all three rendered messages, including which provider each one went through. They run in milliseconds and never touch the network.

## Where we ended up

The `OrderCreated` intent now reaches the buyer three ways:

```text
Ordering
    |
    | OrderCreated
    | order model + buyer reference
    v
Communications
    |
    +-- resolve buyer             Identity
    +-- add push devices          push registration store
    +-- enrich products           Catalog
    |
    v
Enriched content context
    |
    +-- Email  -> Simulation.Email   (SMTP, SendGrid)
    |
    +-- SMS    -> Simulation.Sms     (Twilio)
    |
    +-- Push   -> Simulation.Push    (Firebase)
```

Ordering's code is identical to Part 3's. Everything new lives in Communications:

```text
Recipient addresses
    resolver and profile enrichers

Channel content
    templates per channel

Channel policy
    delivery strategy

Delivery
    providers, one per channel
```

Each of those can change on its own schedule. A new channel is a pipeline change. A new policy is a strategy change. A new provider is a registration change. The intent stays `OrderCreated` through all of it.

There's one word in those log lines worth a second look: `Dispatched`. With the simulator, handing a message to the provider and the message arriving are the same moment. Real providers rarely work that way. Twilio or SendGrid accept the message, queue it, and report back later as it's sent, delivered, bounced, or opened.

In Part 5 we'll handle those delivery events: take the progress updates providers send us, tie each one back to the communication that caused it, and store them, so they can feed logging, support tools, or an inbox where customers can see what we've sent them.
