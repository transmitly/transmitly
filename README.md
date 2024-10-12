# Transactional Communications
Transmitly is a powerful and vendor-agnostic communication library designed to simplify and enhance the process of sending transactional messages across various platforms. With its easy-to-use API, developers can seamlessly integrate email, SMS, and other messaging services into their applications, ensuring reliable and efficient delivery of critical notifications. Built for flexibility and scalability, Transmitly supports multiple communication channels, allowing you to focus on building great applications while it handles the complexity of message transmission.

## Kitchen Sink
Want to jump right into the code? Take a look at the ["Kitchen Sink" Sample Project](/samples/Transmitly.KitchenSink.AspNetCoreWebApi). The kitchen sink is all about showing off the features of how Transmitly can help you with your communications strategy.


### Quick Start
Let's start off where most developers start, sending an email via an SMTP server.
In Transmitly, an Email is what we refer to as a `Channel`. A `channel` is the medium of which your communication will be dispatched. Out of the box, Transmitly supports: `Email`, `SMS`, `Voice`, and `Push`. 

### Add the Transmitly Nuget package to your project
```shell
dotnet add package Transmitly
```

### Choosing a channel provider
As mentioned above, we're going to dispatch our Email using an SMTP server. To make this happen in transmitly, you'll add the [MailKit Channel Provider library](https://github.com/transmitly/transmitly-channel-provider-mailkit) to your project.

`Channel Providers` handle the details of how your `channel` communication will be delivered. You can think of a `Channel Provider` as a service like Twilio, Infobip, Firebase or in this case, an SMTP server.

```shell
dotnet add package Transmitly.ChannelProvider.MailKit
```

### Setup a Pipeline
Now it's time to configure a `pipeline`. `Pipelines` will give us a lot of flexibility down the road. For now you can think of a pipeline as a way to configure which channels and channel providers are involved when you dispatch domain event. 
In other words, you typically start an application by sending a welcome email to a newly registered user. As your application grows, you may want to send an SMS or an Email depending on which address the user gave you at sign up. With Transmitly, it's managed in a single location and your domain/business logic is agnostic of which communications are sent and how.

```csharp
using Transmitly;

ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
.AddMailKitSupport(options =>
{
  options.Host = "smtp.example.com";
  options.Port = 587;
  options.UseSsl = true;
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

    //Dispatch (Send) our configured email
    var result = await _communicationsClient.DispatchAsync("WelcomeKit", "newAccount@gmail.com", new{});

    if(result.IsSuccessful)
      return newAccount;

    throw Exception("Error sending communication!");
  }
}
```

That's it! You're sending emails like a champ. But you might think that seems like a lot of work compared to a simple IEmail Client. Let's break down what we gained by using Transmitly.
 * Vendor agnostic - We can change channel providers with a simple configuration change
   * That means when we want to try out SendGrid, Twilio, Infobip or one of the many other services, it's a single change in a single location. :relaxed: 
 * Delivery configuration - The details of our (Email) communications are not cluttering up our code base.
 * Message composition - The details of how an email or sms is generated are not scattered throughout your codebase.
   * In the future we may want to send an SMS and/or push notifications. We can now control that in a single location -- not in our business logic.
 * We can now use a single service/client for all of our communication needs
   * No more cluttering up your service constructors with IEmailClient, ISmsClient, etc.
  

### Changing Channel Providers
Want to try out a new service to send out your emails? Twilio? Infobip? With Transmitly it's as easy as adding a your prefered channel provider and a few lines of configuration. In the example below, we'll try out SendGrid.

For the next example we'll start using SendGrid to send our emails. 
```shell
dotnet install Transmitly.ChannelProvider.Sendgrid
```

Next we'll update our configuration. Notice we've removed MailKitSupport and added SendGridSupport. 
```csharp
using Transmitly;

ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
//.AddMailKitSupport(options =>
//{
//  options.Host = "smtp.example.com";
//  options.Port = 587;
//  options.UseSsl = true;
//  options.UserName = "MySMTPUsername";
//  options.Password = "MyPassword";
//})
.AddSendGridSupport(options=>
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
.BuildClient();

builder.Services.AddSingleton(communicationsClient);
```

That's right, we added a new channel provider package. Removed our SMTP/MailKit configuration and added and configured our Send Grid support. Notice that no other code needs to change. Our piplines, channel and more importantly our domain/business logic stays the same. :open_mouth:

### Next Steps
We've only scratched the surface. Transmitly can do a **LOT** more to _deliver_ more value for your entire team. [Check out the Kitchen Sink](/samples/Transmitly.KitchenSink.AspNetCoreWebApi) sample to learn more about Transmitly's concepts while we work on improving our [wiki](https://github.com/transmitly/transmitly/wiki).

## Supported Channel Providers

| Channel(s)  | Project | 
| ------------- | ------------- |
| Email  | [Transmitly.ChannelProvider.MailKit](https://github.com/transmitly/transmitly-channel-provider-mailkit)  |
| Email  | [Transmitly.ChannelProvider.SendGrid](https://github.com/transmitly/transmitly-channel-provider-sendgrid)  |
| Email, Sms, Voice | [Transmitly.ChannelProvider.InfoBip](https://github.com/transmitly/transmitly-channel-provider-infobip)  |
| Sms, Voice  | [Transmitly.ChannelProvider.Twilio](https://github.com/transmitly/transmitly-channel-provider-twilio)  |
| Push Notifications  | [Transmitly.ChannelProvider.Firebase](https://github.com/transmitly/transmitly-channel-provider-firebase)  |

## Supported Template Engines
| Project |
| ------------- |
| [Transmitly.TemplateEngine.Fluid](https://github.com/transmitly/transmitly-template-engine-fluid)  |
| [Transmitly.TemplateEngine.Scriban](https://github.com/transmitly/transmitly-template-engine-scriban)  |

## Supported Dependency Injection Containers
|Container |  Project |
| -------- | -------- |
| Microsoft.Microsoft.Extensions.DependencyInjection | [Transmitly.Microsoft.Extensions.DependencyInjection](https://github.com/transmitly/transmitly-microsoft-extensions-dependencyinjection)  |

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://github.com/transmitly/transmitly/assets/3877248/524f26c8-f670-4dfa-be78-badda0f48bfb">
  <img alt="an open-source project sponsored by CiLabs of Code Impressions, LLC" src="https://github.com/transmitly/transmitly/assets/3877248/34239edd-234d-4bee-9352-49d781716364" width="500" align="right">
</picture> 

---------------------------------------------------

_Copyright &copy; Code Impressions, LLC - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
