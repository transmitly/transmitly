# Transactional Communications
Transmitly is a powerful and vendor-agnostic communication library designed to simplify and enhance the process of sending transactional messages across various platforms. With its easy-to-use API, developers can seamlessly integrate email, SMS, and other messaging services into their applications, ensuring reliable and efficient delivery of critical notifications. Built for flexibility and scalability, Transmitly supports multiple communication channels, allowing you to focus on building great applications while it handles the complexity of message transmission.

## Show me the code!
Want to jump right into the code? Take a look at the [various sample projects](https://github.com/transmitly/transmitly/tree/main/samples).


### Quick Start
Let's begin where most developers start: sending an email via an SMTP server.
In Transmitly, Email is a `Channel`. A `Channel` is the medium through which your communication is dispatched. Out of the box, Transmitly supports `Email`, `SMS`, `Voice`, and `Push Notifications`.

### Add the Transmitly NuGet package to your project
```shell
dotnet add package Transmitly
```

### Choosing a channel provider
As mentioned above, we're going to dispatch our email using an SMTP server. To make this happen in Transmitly, add the [SMTP Channel Provider library](https://github.com/transmitly/transmitly-channel-provider-smtp) to your project.

`Channel Providers` manage the delivery of your `channel` communications. You can think of a `Channel Provider` as a service like Twilio, Infobip, Firebase, or in this case, an SMTP server.

```shell
dotnet add package Transmitly.ChannelProvider.Smtp
```

### Set Up a Pipeline
Now it's time to configure a `Pipeline`. `Pipelines` give us flexibility as requirements evolve. For now, think of a `Pipeline` as a way to configure which channels and channel providers are involved when you dispatch a domain intent.
In other words, you might start by sending a welcome email to a newly registered user. As your application grows, you may want to send an SMS or an email depending on which address the user provided at sign-up. With Transmitly, that behavior is managed in a single location, and your domain/business logic remains agnostic of which communications are sent and how.

```csharp
using Transmitly;

ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
.AddSmtpSupport(options =>
{
  options.Host = "smtp.example.com";
  options.Port = 587;
  options.UserName = "MySMTPUsername";
  options.Password = "MyPassword";
})
.AddPipeline("WelcomeKit", pipeline =>
{
    pipeline.AddEmail("welcome@my.app".AsIdentityAddress("Welcome Committee"), email =>
    {
       email.Subject.AddStringTemplate("Thanks for creating an account!");
       email.HtmlBody.AddStringTemplate("Check out the <a href=\"https://my.app/getting-started\">Getting Started</a> section to see all the cool things you can do!");
       email.TextBody.AddStringTemplate("Check out the Getting Started (https://my.app/getting-started) section to see all the cool things you can do!");
    });
})
.BuildClient();

// Register ICommunicationsClient with the service collection.
// If you install Transmitly.Microsoft.Extensions.DependencyInjection,
// you can use builder.Services.AddTransmitly(...) instead of manual registration.
builder.Services.AddSingleton(communicationsClient);
```
In our new account registration code:
```csharp
class AccountRegistrationService
{
  private readonly ICommunicationsClient _communicationsClient;
  public AccountRegistrationService(ICommunicationsClient communicationsClient)
  {
    _communicationsClient = communicationsClient;
  }

  public async Task<Account> RegisterNewAccount(AccountVM account)
  {
    // Validate and create the account
    var newAccount = CreateAccount(account);

    // Dispatch (send) using our configured pipeline intent and recipient email address.
    var result = await _communicationsClient.DispatchAsync("WelcomeKit", newAccount.EmailAddress, new { });

    if(result.IsSuccessful)
      return newAccount;

    throw new Exception("Error sending communication!");
  }
}
```

That's it. You're dispatching email. It may seem like a lot of work compared to a simple `IEmailClient`, so let's break down what we gained by using Transmitly.
 * Vendor agnostic - We can change channel providers with a simple configuration change
   * That means when we want to try out SendGrid, Twilio, Infobip or one of the many other services, it's a single change in a single location. :relaxed: 
 * Delivery configuration - The details of our (Email) communications are not cluttering up our code base.
   * We've also managed to keep our domain/business logic clean by using pipeline intents rather than explicitly sending an email or other communication types.
 * Message composition - The details of how an email or SMS is generated are not scattered throughout your codebase.
   * In the future we may want to send an SMS and/or push notification. We can now control that in a single location, not in our business logic.
 * We can now use a single service/client for all of our communication needs
   * No more cluttering up your service constructors with `IEmailClient`, `ISmsClient`, etc.
  

### Changing Channel Providers
Want to try out a new service to send your emails? Twilio? Infobip? With Transmitly, it's as easy as adding your preferred channel provider and a few lines of configuration. In the example below, we'll use SendGrid.

For the next example, we'll start using SendGrid to send email.
```shell
dotnet add package Transmitly.ChannelProvider.SendGrid
```

Next we'll update our configuration. Notice we've removed `AddSmtpSupport(...)` and added `AddSendGridSupport(...)`.
```csharp
using Transmitly;

ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
//.AddSmtpSupport(options =>
//{
//  options.Host = "smtp.example.com";
//  options.Port = 587;
//  options.UserName = "MySMTPUsername";
//  options.Password = "MyPassword";
//})
.AddSendGridSupport(options =>
{
    options.ApiKey = "MySendGridApi";
})
.AddPipeline("WelcomeKit", pipeline =>
{
    pipeline.AddEmail("welcome@my.app".AsIdentityAddress("Welcome Committee"), email =>
    {
        email.Subject.AddStringTemplate("Thanks for creating an account!");
        email.HtmlBody.AddStringTemplate("Check out the <a href=\"https://my.app/getting-started\">Getting Started</a> section to see all the cool things you can do!");
        email.TextBody.AddStringTemplate("Check out the Getting Started (https://my.app/getting-started) section to see all the cool things you can do!");
    });
})
.BuildClient();

builder.Services.AddSingleton(communicationsClient);
```

That's right, we added a new channel provider package, removed our SMTP configuration, and configured SendGrid support. You don't need to change any other code. Our pipelines, channels, and domain/business logic stay the same. :open_mouth:

### Supported Channel Providers

| Channel(s)  | Project | Package |
| ------------- | ------------- | -------- | 
| Email  | [Transmitly.ChannelProvider.Smtp](https://github.com/transmitly/transmitly-channel-provider-smtp)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Smtp?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Smtp) |
| Email  | [Transmitly.ChannelProvider.SendGrid](https://github.com/transmitly/transmitly-channel-provider-sendgrid)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.SendGrid?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.SendGrid) |
| Email  | [Transmitly.ChannelProvider.Mailgun](https://github.com/transmitly/transmitly-channel-provider-mailgun)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Mailgun?style=flat&color=01aef0&logo=mailgun)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Mailgun) |
| Email, Sms, Voice | [Transmitly.ChannelProvider.Infobip](https://github.com/transmitly/transmitly-channel-provider-infobip)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Infobip?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Infobip) |
| Sms, Voice  | [Transmitly.ChannelProvider.Twilio](https://github.com/transmitly/transmitly-channel-provider-twilio)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Twilio?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Twilio) |
| Push Notifications  | [Transmitly.ChannelProvider.Firebase](https://github.com/transmitly/transmitly-channel-provider-firebase)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.ChannelProvider.Firebase?style=flat&color=01aef0&logo=firebase)](https://www.nuget.org/packages/Transmitly.ChannelProvider.Firebase) | 

### Delivery Reports
Now that we are dispatching communications, the next questions are usually: how do I log activity, how do I store content, and what about status updates from third-party services? All great questions. To start, we'll focus on logging requests. In this simple example, we're using the SMTP library, which provides limited delivery visibility compared to third-party channel providers. As you adopt those providers, you can access richer dispatch and delivery data. Delivery reports let you manage those updates in a structured, consistent way across any channel provider or channel.

```csharp
using Transmitly;

ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
.AddSendGridSupport(options=>
{
    options.ApiKey = "MySendGridApi";
})
.AddDeliveryReportHandler((report) =>
{
   logger.LogInformation("[{channelId}:{channelProviderId}:Dispatched] Id={id}; Content={communication}", report.ChannelId, report.ChannelProviderId, report.ResourceId, JsonSerializer.Serialize(report.ChannelCommunication));
   return Task.CompletedTask;
})
.AddPipeline("WelcomeKit", pipeline =>
{
    pipeline.AddEmail("welcome@my.app".AsIdentityAddress("Welcome Committee"), email =>
    {
       email.Subject.AddStringTemplate("Thanks for creating an account!");
       email.HtmlBody.AddStringTemplate("Check out the <a href=\"https://my.app/getting-started\">Getting Started</a> section to see all the cool things you can do!");
       email.TextBody.AddStringTemplate("Check out the Getting Started (https://my.app/getting-started) section to see all the cool things you can do!");
    });
})
.BuildClient();

builder.Services.AddSingleton(communicationsClient);
```

Adding `AddDeliveryReportHandler(...)` lets you pass a function that executes during different stages of the communication lifecycle. In this case, we're listening to any report for any channel/channel provider. If you'd like more [fine-grained control, check out the wiki](https://github.com/transmitly/transmitly/wiki/Delivery-Reports#filters) for details on filtering the data you want. Delivery reports are designed to give you flexibility as your communications strategy evolves. For example, you can retry failed sends, notify stakeholders of important messages, or store dispatched content for auditing.

Note: As mentioned earlier, using third-party services usually means you'll receive asynchronous status updates. In general, most providers push this information via webhooks. Transmitly can help with webhook handling through the MVC libraries.

Using the Transmitly MVC libraries, you can configure channel providers to send webhook updates to a single endpoint you define. Transmitly then wraps provider-specific payloads and invokes your registered delivery report handlers.

- [AspNetCore.Mvc](https://github.com/transmitly/transmitly-microsoft-aspnetcore-mvc) [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.Microsoft.AspnetCore.Mvc?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.Microsoft.AspnetCore.Mvc)
- [AspNet.Mvc](https://github.com/transmitly/transmitly-microsoft-aspnet-mvc) [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.Microsoft.Aspnet.Mvc?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.Microsoft.Aspnet.Mvc)

[See the wiki for more on delivery reports](https://github.com/transmitly/transmitly/wiki/Delivery-Reports)

### Template Engines
Templating is not supported out of the box by design. This lets you choose the engine you prefer, including a bespoke engine you may already use. Today, Transmitly has two officially supported template engines: Fluid and Scriban. As with other features, setup is as simple as adding the package to your project. For this example, we'll use Scriban.

```bash
dotnet add package Transmitly.TemplateEngine.Scriban
```

Building on our example, we can enable support by adding `AddScribanTemplateEngine()`. We'll also update our email templates to use placeholders.

```csharp
using Transmitly;

ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
.AddSendGridSupport(options=>
{
    options.ApiKey = "MySendGridApi";
})
.AddScribanTemplateEngine()
.AddDeliveryReportHandler((report) =>
{
   logger.LogInformation("[{channelId}:{channelProviderId}:Dispatched] Id={id}; Content={communication}", report.ChannelId, report.ChannelProviderId, report.ResourceId, JsonSerializer.Serialize(report.ChannelCommunication));
   return Task.CompletedTask;
})
.AddPipeline("WelcomeKit", pipeline =>
{
    pipeline.AddEmail("welcome@my.app".AsIdentityAddress("Welcome Committee"), email =>
    {
       email.Subject.AddStringTemplate("Thanks for creating an account, {{firstName}}!");
       email.HtmlBody.AddStringTemplate("{{firstName}}, check out the <a href=\"https://my.app/getting-started\">Getting Started</a> section to see all the cool things you can do!");
       email.TextBody.AddStringTemplate("{{firstName}}, check out the Getting Started (https://my.app/getting-started) section to see all the cool things you can do!");
    });
})
.BuildClient();

builder.Services.AddSingleton(communicationsClient);
```
We'll also update our dispatch call to provide a transactional model for the template engine.

```csharp
class AccountRegistrationService
{
  private readonly ICommunicationsClient _communicationsClient;
  public AccountRegistrationService(ICommunicationsClient communicationsClient)
  {
    _communicationsClient = communicationsClient;
  }

  public async Task<Account> RegisterNewAccount(AccountVM account)
  {
    // Validate and create the account
    var newAccount = CreateAccount(account);

    // Dispatch (send) our configured email
    var result = await _communicationsClient.DispatchAsync("WelcomeKit", newAccount.EmailAddress, new { firstName = newAccount.FirstName });

    if(result.IsSuccessful)
      return newAccount;

    throw new Exception("Error sending communication!");
  }
}
```

That's another advanced feature handled in a strongly typed and extensible way. In this example, we only added `firstName` to our model. If we wanted to be more future-proof for template changes, we could return the full `Account` object or, preferably, create and use a `Platform Identity Resolver`. Whether you're starting from scratch or adapting an existing communications strategy, there's an approach that will work for you.

### Supported Template Engines
| Project | Package |
| ------------- | ----------- |
| [Transmitly.TemplateEngine.Fluid](https://github.com/transmitly/transmitly-template-engine-fluid)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.TemplateEngine.Fluid?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.TemplateEngine.Fluid) |
| [Transmitly.TemplateEngine.Scriban](https://github.com/transmitly/transmitly-template-engine-scriban)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.TemplateEngine.Scriban?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.TemplateEngine.Scriban) |


### Next Steps
We've only scratched the surface. Transmitly can do a lot more to deliver value for your team. [Check out the Kitchen Sink](/samples/Transmitly.KitchenSink.AspNetCoreWebApi) sample to learn more about Transmitly concepts while we continue improving the [wiki](https://github.com/transmitly/transmitly/wiki).

### Supported Dependency Injection Containers
|Container |  Project | Package | 
| -------- | -------- | --------- |
| Microsoft.Extensions.DependencyInjection | [Transmitly.Microsoft.Extensions.DependencyInjection](https://github.com/transmitly/transmitly-microsoft-extensions-dependencyinjection)  | [![NuGet Version](https://img.shields.io/nuget/v/Transmitly.Microsoft.Extensions.DependencyInjection?style=flat&color=01aef0)](https://www.nuget.org/packages/Transmitly.Microsoft.Extensions.DependencyInjection) |

---
_Copyright (c) Code Impressions, LLC.  This open-source project is sponsored and maintained by Code Impressions
and is licensed under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._

