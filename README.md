# Transactional Communications
Transmitly™ is a powerful and vendor-agnostic communication library designed to simplify and enhance the process of sending transactional messages across various platforms. With its easy-to-use API, developers can seamlessly integrate email, SMS, and other messaging services into their applications, ensuring reliable and efficient delivery of critical notifications. Built for flexibility and scalability, Transmitly supports multiple communication channels, allowing you to focus on building great applications while it handles the complexity of message transmission.

## Show me the code!
Want to jump right into the code? Take a look at the [various sample projects](https://github.com/transmitly/transmitly/tree/main/samples).


### Quick Start
Let's begin where most developers start, sending an email via an SMTP server.
In Transmitly, an Email is a `Channel`. A `channel` is the medium of which your communication will be dispatched. Out of the box, Transmitly supports `Email`, `SMS`, `Voice`, and `Push`. 

### Add the Transmitly Nuget package to your project
```shell
dotnet add package Transmitly
```

### Choosing a channel provider
As mentioned above, we're going to dispatch our Email using an SMTP server. To make this happen in transmitly, you'll add the [SMTP Channel Provider library](https://github.com/transmitly/transmitly-channel-provider-smtp) to your project.

`Channel Providers` manage the delivery of your `channel` communication. You can think of a `Channel Provider` as a service like Twilio, Infobip, Firebase or in this case, an SMTP server.

```shell
dotnet add package Transmitly.ChannelProvider.Smtp
```

### Setup a Pipeline
Now it's time to configure a `pipeline`. `Pipelines` will give us a lot of flexibility down the road. For now you can think of a pipeline as a way to configure which channels and channel providers are involved when you dispatch your domain intent. 
In other words, you typically start an application by sending a welcome email to a newly registered user. As your application grows, you may want to send an SMS or an Email depending on which address the user gave you at sign up. With Transmitly, it's managed in a single location and your domain/business logic is agnostic of which communications are sent and how.

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

//In this case, we're using Microsoft.DependencyInjection. We need to register our `ICommunicationsClient` with the service collection
//Tip: The Microsoft Dependency Injection library will take care of the registration for you (https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection)
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
    //Validate and create the Account
    var newAccount = CreateAccount(account);

    //Dispatch (Send) using our pipeline intent named and the email address of the new account
    var result = await _communicationsClient.DispatchAsync("WelcomeKit", newAccount.EmailAddress, new{});

    if(result.IsSuccessful)
      return newAccount;

    throw Exception("Error sending communication!");
  }
}
```

That's it! You're dispatching emails like a champ. But you might think that seems like a lot of work compared to a simple IEmail Client. Let's break down what we gained by using Transmitly.
 * Vendor agnostic - We can change channel providers with a simple configuration change
   * That means when we want to try out SendGrid, Twilio, Infobip or one of the many other services, it's a single change in a single location. :relaxed: 
 * Delivery configuration - The details of our (Email) communications are not cluttering up our code base.
   * We've also managed to keep our domain/business logic clean by using pipeline intents rather than explicitly sending and email or other communication types. 
 * Message composition - The details of how an email or sms is generated are not scattered throughout your codebase.
   * In the future we may want to send an SMS and/or push notifications. We can now control that in a single location -- not in our business logic.
 * We can now use a single service/client for all of our communication needs
   * No more cluttering up your service constructors with IEmailClient, ISmsClient, etc.
  

### Changing Channel Providers
Want to try out a new service to send out your emails? Twilio? Infobip? With Transmitly it's as easy as adding a your preferred channel provider and a few lines of configuration. In the example below, we'll try out SendGrid.

For the next example we'll start using SendGrid to send our emails. 
```shell
dotnet install Transmitly.ChannelProvider.Sendgrid
```

Next we'll update our configuration. Notice we've removed SmtpSupport and added SendGridSupport. 
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

That's right, we added a new channel provider package. Removed our SMTP configuration and added and configured our Send Grid support. You don't need to change any other code. Our pipelines, channel and more importantly our domain/business logic stays the same. :open_mouth:

### Supported Channel Providers

| Channel(s)  | Project | 
| ------------- | ------------- |
| Email  | [Transmitly.ChannelProvider.Smtp](https://github.com/transmitly/transmitly-channel-provider-smtp)  |
| Email  | [Transmitly.ChannelProvider.SendGrid](https://github.com/transmitly/transmitly-channel-provider-sendgrid)  |
| Email, Sms, Voice | [Transmitly.ChannelProvider.InfoBip](https://github.com/transmitly/transmitly-channel-provider-infobip)  |
| Sms, Voice  | [Transmitly.ChannelProvider.Twilio](https://github.com/transmitly/transmitly-channel-provider-twilio)  |
| Push Notifications  | [Transmitly.ChannelProvider.Firebase](https://github.com/transmitly/transmitly-channel-provider-firebase)  |

### Delivery Reports
Now that we are dispatching communications, the next question is along the lines of: how do I log things; how do I store the content; what about status updates from the 3rd party services? All great questions. To start, we'll focus on logging the requests. Our simple example is using the SMTP library. In that case we don't get a lot of visibility into if it was sent. Just that it was dispatched or delivered. Once you move into 3rd party channel providers you start to unlock more fidelity into what is and has happened to your communications. Delivery reports are how you manage these updates in a structured and consistent way across any channel provider or channel. 

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
.BuildClient();

builder.Services.AddSingleton(communicationsClient);
```

Adding the `AddDeliveryReportHandler` gives us the option of passing in a func that will be executed during different lifecycles of the communications being dispatched. In this case, we're listening to any report for any channel/channel provider. If you'd like a bit more [fine grained control check out the wiki](https://github.com/transmitly/transmitly/wiki/Delivery-Reports#filters) for information on how you can dial in the data you want. Delivery reports are built to give you the most flexibility to handle the changes to communications as part of your communications strategy. With a delivery report you could retry a failed send, notify stakeholders of important messages and more commonly, store the contents of communications being sent.

Note: As mentioned earlier, using 3rd party services usually means you will have asynchronous updates to the status of the communication. In general, most providers will push this information to you in the form of a webhook. Transmitly can help with these webhooks with the Mvc libraries.

Using the Transmitly Mvc libraries you're able to configure all of your channel providers to send to the endpoint you define. Transmitly will manage wrapping the data up and calling your delivery report handlers. [[AspNetCore.Mvc](https://github.com/transmitly/transmitly-microsoft-aspnetcore-mvc)] [[AspNet.Mvc](https://github.com/transmitly/transmitly-microsoft-aspnet-mvc)]

[See the wiki for more on delivery reports]([wiki/Delivery-Reports](https://github.com/transmitly/transmitly/wiki/Delivery-Reports))

### Template Engines
Templating is not supported out of the box. This is by design to allow you to choose the template engine you prefer, or even further, integrating a bespoke engine that you'd really like to keep using. As of today, Transmitly has two officially supported template engines; Fluid & Scriban. As with any other feature, it's as simple as adding the template engine to your project. For this example, we'll use Scriban

```bash
dotnet add Transmitly.TemplateEngines.Scriban
```

Building upon our example, we can add support by adding the `AddScribanTemplateEngine()`. Along with adding the template engine, we'll want to update date our email template to actually do some templating

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
.BuildClient();

builder.Services.AddSingleton(communicationsClient);
```
and we'll also update our Dispatch call to provide a transactional model for the template engine to use.

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
    //Validate and create the Account
    var newAccount = CreateAccount(account);

    //Dispatch (Send) our configured email
    var result = await _communicationsClient.DispatchAsync("WelcomeKit", newAccount.EmailAddress, new { firstName = newAccount.FirstName });

    if(result.IsSuccessful)
      return newAccount;

    throw Exception("Error sending communication!");
  }
}
```

That's another fairly advanced feature handled in a strongly typed and extensible way. In this example, we only added the `firstName` to our model. If we wanted to be even more future proof to template changes, we could have returned the `Account` object or preferably create and used a `Platform Identity Resolver`. Whether you are starting from scratch or working around an existing communications strategy, there's an approach that will work for you.

### Supported Template Engines
| Project |
| ------------- |
| [Transmitly.TemplateEngine.Fluid](https://github.com/transmitly/transmitly-template-engine-fluid)  |
| [Transmitly.TemplateEngine.Scriban](https://github.com/transmitly/transmitly-template-engine-scriban)  |

### Next Steps
We've only scratched the surface. Transmitly can do a **LOT** more to _deliver_ more value for your entire team. [Check out the Kitchen Sink](/samples/Transmitly.KitchenSink.AspNetCoreWebApi) sample to learn more about Transmitly's concepts while we work on improving our [wiki](https://github.com/transmitly/transmitly/wiki).

### Supported Dependency Injection Containers
|Container |  Project |
| -------- | -------- |
| Microsoft.Microsoft.Extensions.DependencyInjection | [Transmitly.Microsoft.Extensions.DependencyInjection](https://github.com/transmitly/transmitly-microsoft-extensions-dependencyinjection)  |


### Copyright and Trademark 

Copyright © 2024–2025 Code Impressions, LLC.

Transmitly™ is a trademark of Code Impressions, LLC.

This open-source project is sponsored and maintained by Code Impressions
and is licensed under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html).

The Apache License applies to the software code only and does not grant
permission to use the Transmitly name or logo, except as required to
describe the origin of the software.
