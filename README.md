Let your .NET application express it's intent to communicate without doing the work in your business logic.

[![NuGet](https://img.shields.io/nuget/v/Transmitly?label=NuGet)](https://www.nuget.org/packages/Transmitly)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Transmitly)](https://www.nuget.org/packages/Transmitly)
[![License](https://img.shields.io/github/license/transmitly/transmitly)](https://github.com/transmitly/transmitly/blob/main/LICENSE)

Transmitly is an extensible transactional communications library for .NET. It keeps email, SMS, push, voice, message composition, provider configuration, and delivery handling out of your domain and application logic.

Your application expresses an intent:

```csharp
var result = await _communicationsClient.DispatchAsync(
    "WelcomeKit",
    newAccount.EmailAddress,
    new { firstName = newAccount.FirstName });
```

Elsewhere, Transmitly defines what `WelcomeKit` means: which channels are involved, how content is composed, which providers can deliver it, and how delivery events are handled.

```text
Application
    |
    | Dispatch("WelcomeKit")
    v
Pipeline: WelcomeKit
    |
    +-- Email ----> SMTP / SendGrid / Mailgun / Infobip
    +-- SMS ------> Twilio / Infobip
    +-- Push -----> Firebase
    +-- Voice ----> Twilio / Infobip
```

That separation is the point of Transmitly.

## When should I use it?

Transmitly is a good fit when:

- transactional communication is becoming infrastructure rather than a single `SendEmail(...)` call
- provider SDKs, templates, channel decisions, or webhook behavior are leaking into application code
- you have, or expect, multiple message types, channels, or providers
- you want composition and delivery behavior managed in one place
- you want provider or channel changes without rewriting the application workflow that requested the communication

### When shouldn't I use it?

If your application sends one kind of email through one provider and that is unlikely to change, a small `IEmailSender`-style abstraction may be enough.

## Three concepts to get started

- **Pipeline** — a domain-oriented communication intent such as `WelcomeKit`, `PasswordReset`, or `OrderProcessing`
- **Channel** — how the recipient can receive it: Email, SMS, Voice, or Push Notification
- **Channel Provider** — the infrastructure that delivers the channel, such as SMTP, SendGrid, Twilio, Infobip, Mailgun, or Firebase

The pipeline is the stable boundary your application talks to. Channels, providers, templates, and delivery behavior can evolve behind it.

## Try Transmitly without a provider account

The core package includes simulation support, so your first dispatch does not require an SMTP server, API key, or third-party account.

```shell
dotnet add package Transmitly
```

```csharp
using Transmitly;

var communicationsClient = new CommunicationsClientBuilder()
    .AddSimulationSupport()
    .AddPipeline("WelcomeKit", pipeline =>
    {
        pipeline.AddEmail(
            "welcome@my.app".AsIdentityAddress("Welcome"),
            email =>
            {
                email.Subject.AddStringTemplate("Thanks for signing up!");
                email.TextBody.AddStringTemplate("Welcome to the app.");
            });
    })
    .BuildClient();

var result = await communicationsClient.DispatchAsync(
    "WelcomeKit",
    "developer@example.com",
    new { });

Console.WriteLine(result.IsSuccessful);
```

`AddSimulationSupport()` uses Transmitly's built-in simulation provider. By default, it returns a successful simulated dispatch result so you can exercise the same pipeline model without sending a real communication.

## Send through a real provider

When you're ready for real delivery, install only the provider your application uses. For SMTP:

```shell
dotnet add package Transmitly.ChannelProvider.Smtp
```

```csharp
var communicationsClient = new CommunicationsClientBuilder()
    .AddSmtpSupport(options =>
    {
        options.Host = "smtp.example.com";
        options.Port = 587;
        options.UserName = "MySMTPUsername";
        options.Password = "MyPassword";
    })
    .AddPipeline("WelcomeKit", pipeline =>
    {
        pipeline.AddEmail(
            "welcome@my.app".AsIdentityAddress("Welcome"),
            email => email.Subject.AddStringTemplate("Thanks for signing up!"));
    })
    .BuildClient();
```

Your application still dispatches `WelcomeKit`. It does not become SMTP-specific.

### Change providers without changing application intent

Moving email delivery from SMTP to SendGrid is a provider configuration change:

```diff
- .AddSmtpSupport(options =>
- {
-     options.Host = "smtp.example.com";
-     options.Port = 587;
- })
+ .AddSendGridSupport(options =>
+ {
+     options.ApiKey = "MySendGridApi";
+ })
```

After installing `Transmitly.ChannelProvider.SendGrid`, the pipeline and calling application can remain unchanged.

## The ecosystem is modular by design

Most applications need the Transmitly core package, one or more provider integrations, and optionally a template integration.

### Channel providers

| Channel(s) | Integration | Package |  |
| --- | --- | --- | --- |
| Email | [SMTP](https://github.com/transmitly/transmitly-channel-provider-smtp) | [Transmitly.ChannelProvider.Smtp](https://www.nuget.org/packages/Transmitly.ChannelProvider.Smtp) | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Smtp?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Smtp) |
| Email | [SendGrid](https://github.com/transmitly/transmitly-channel-provider-sendgrid) | [Transmitly.ChannelProvider.SendGrid](https://www.nuget.org/packages/Transmitly.ChannelProvider.SendGrid) | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.SendGrid?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.SendGrid) |
| Email | [Mailgun](https://github.com/transmitly/transmitly-channel-provider-mailgun) | [Transmitly.ChannelProvider.Mailgun](https://www.nuget.org/packages/Transmitly.ChannelProvider.Mailgun) | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Mailgun?style=flat&color=01aef0&logo=mailgun)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Mailgun) |
| Email, SMS, Voice | [Infobip](https://github.com/transmitly/transmitly-channel-provider-infobip) | [Transmitly.ChannelProvider.Infobip](https://www.nuget.org/packages/Transmitly.ChannelProvider.Infobip) | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Infobip?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Infobip) |
| SMS, Voice | [Twilio](https://github.com/transmitly/transmitly-channel-provider-twilio) | [Transmitly.ChannelProvider.Twilio](https://www.nuget.org/packages/Transmitly.ChannelProvider.Twilio) | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Twilio?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Twilio) |
| Push Notifications | [Firebase](https://github.com/transmitly/transmitly-channel-provider-firebase) | [Transmitly.ChannelProvider.Firebase](https://www.nuget.org/packages/Transmitly.ChannelProvider.Firebase) | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Firebase?style=flat&color=01aef0&logo=firebase)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Firebase) | 

### Optional integrations

- [Microsoft.Extensions.DependencyInjection](https://github.com/transmitly/transmitly-microsoft-extensions-dependencyinjection)
- [ASP.NET Core MVC](https://github.com/transmitly/transmitly-microsoft-aspnetcore-mvc) and [AspNet.Mvc](https://github.com/transmitly/transmitly-microsoft-aspnet-mvc) for provider webhook/delivery-report integration
- [Fluid](https://github.com/transmitly/transmitly-template-engine-fluid) and [Scriban](https://github.com/transmitly/transmitly-template-engine-scriban) for dynamic template rendering

Literal/string message content can be configured directly in core. Dynamic template rendering is deliberately pluggable so you can choose the engine that fits your application.

## Delivery reports

Transmitly provides a consistent delivery-report model so application-level handling does not have to become provider-specific. Third-party providers commonly send later status updates through webhooks; the optional MVC integrations can route those provider-specific payloads into Transmitly's delivery-report handling.

- [Delivery reports documentation](https://github.com/transmitly/transmitly/wiki/Delivery-Reports)
- [ASP.NET Core MVC integration](https://github.com/transmitly/transmitly-microsoft-aspnetcore-mvc)
- [AspNet.Mvc integration](https://github.com/transmitly/transmitly-microsoft-aspnet-mvc) 

## Compatibility

The core project currently targets:

- .NET 10
- .NET 9
- .NET 8
- .NET 6
- .NET Standard 2.0
- .NET Framework 4.8 and 4.7.2

## Project status

**Transmitly 0.4.0 is production-ready and in production use.**

There are additions and refinements planned before `1.0`, and the project still reserves room for API evolution while that work lands.

## Samples and documentation

- [Samples](https://github.com/transmitly/transmitly/tree/main/samples)
- [Kitchen Sink sample](https://github.com/transmitly/transmitly/tree/main/samples/Transmitly.KitchenSink.AspNetCoreWebApi)
- [Microservices sample](https://github.com/transmitly/transmitly/tree/main/samples/Microservices)
- [Wiki](https://github.com/transmitly/transmitly/wiki)

## Feedback wanted

If you try the quick start and something is confusing, overly abstract, missing, or simply not worth the ceremony, please tell us in [GitHub Discussions](https://github.com/transmitly/transmitly/discussions). API criticism, onboarding friction, unsupported workflows, and *"I stopped here because this didn't make sense"* are all useful feedback.

For larger proposed changes, starting with a Discussion is the best way to align before implementation.

## License

Licensed under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html).

---
Copyright (c) Code Impressions, LLC
