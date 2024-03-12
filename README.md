# Transmitly - Communications, done right

## Supported Channel Providers

| Channel(s)  | Project | 
| ------------- | ------------- |
| Email  | [Transmitly.ChannelProvider.MailKit](https://github.com/transmitly/transmitly-channel-provider-mailkit)  |
| Email  | [Transmitly.ChannelProvider.SendGrid](https://github.com/transmitly/transmitly-channel-provider-sendgrid)  |
| Email, Sms  | [Transmitly.ChannelProvider.InfoBip](https://github.com/transmitly/transmitly-channel-provider-infobip)  |
| Sms  | [Transmitly.ChannelProvider.Twilio](https://github.com/transmitly/transmitly-channel-provider-twilio)  |
| Push Notifications  | [Transmitly.ChannelProvider.Firebase](https://github.com/transmitly/transmitly-channel-provider-firebase)  |

## Supported Template Engines
| Project |
| ------------- |
| [Transmitly.TemplateEngine.Fluid](https://github.com/transmitly/transmitly-template-engine-fluid)  |
| [Transmitly.TemplateEngine.Scriban](https://github.com/transmitly/transmitly-template-engine-scriban)  |

## Supported Containers
|Container |  Project |
| -------- | -------- |
| [Microsoft.Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection) | [Transmitly.Microsoft.Extensions.DependencyInjection](https://github.com/transmitly/transmitly-microsoft-extensions-dependencyinjection)  |



## Quick Start
Transmitly can do a lot. It's a little overwhelming.
Start by viewing the ["Kitchen Sink" Sample Project](/samples/Transmitly.KitchenSink.AspNetCoreWebApi) or stick around and go through the intoductory tutorial.

### Tutorial
Let's start with where we all generally start in our applications, sending an email. For this example we'll send an email to someone that has just signed up for our cool new app.

### Get Transmitly
```shell
dotnet add package Transmitly
```

### Get a channel provider
Transmitly comes with `channels` like, `Email`, `SMS` and `Push Notifications` out of the box. However, we're going to need a `Channel Provider` to do the heavy lifting of actually sending or dispatching a communication for us. As this is our first email, we'll choose SMTP. In the past we've used MailKit ([Microsoft recommends it!](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=net-8.0#remarks)) to send emails. Let's add that to our project...

```shell
dotnet add package Transmitly.ChannelProvider.MailKit
```

### Configure our email
Now the fun part. In our Startup code we can now define a `pipeline`. Pipelines will give us a lot of flexibility down the road. For now we'll, use one of the MailKit convenient extension methods to keep things simple.

```csharp
using Transmitly;

//CommunicationsClientBuilder is a fluent way to configure our communication settings and pipline
ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
//Transmitly.ChannelProvider.MailKit adds on to the client builder with it's own extensions to make adding setup a breeze
.AddMailKitSupport(options =>
{
  options.Host = "smtp.example.com";
  options.Port = 587;
  options.UseSsl = true;
  options.UserName = "MySMTPUsername";
  options.Password = "MyPassword";
})
//We're keeping it simple here, we're going to add a single email named "NewAccountRegisteration"
.AddEmailMessage("NewAccountRegistration", "noreply@example.com", "Account Created!", "Welcome aboard! Take a look around the <a href=\"https://transmit.ly\">site</a>")
//We're done configuring, now we need to create our new communications client
.BuildClient();

//In this case, we're using Microsoft.DependencyInjection. We need to register our `ICommunicationsClient` with the service collection
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
    var result = await _communicationsClient.DispatchAsync("NewAccountRegistration", "newAccount@gmail.com", new{});

    if(!result.Results.Any(a=>a.DispatchStatus == DispatchStatus.Error))
      return newAccount;

    throw Exception("Error sending communication!");
  }
}
```

That's it. But what did we do? 
 * Externalized delivery configuration - The details of our (Email) communications are not cluttering up our code base.
   * The added benefit is, in the future, we can change it to SendGrid, MailChimp, Infobip or the many other available providers.
 * Externalized message composition - The details of how an email or sms is generated are not scattered throughout your codebase.
   * In the future we may want to send an SMS and/or push notifications. We can now control that in a single location.
 * We can now use a single service/client for all of our communication needs
   * No more cluttering up your service constructors with IEmailClient, ISmsClient, etc.
   * This also cleans up having if/else statement littered to manage our user's communication preferences 

### Next Steps
_**(coming soon)**_ We've only scratched the surface. Transmitly can do a **LOT** more to _deliver_ more value for your entire team. Check out the wiki to learn more about Transmitly's concepts as well as check out our examples to help you get started fast.


<picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://github.com/transmitly/transmitly/assets/3877248/524f26c8-f670-4dfa-be78-badda0f48bfb">
  <img alt="an open-source project sponsored by CiLabs of Code Impressions, LLC" src="https://github.com/transmitly/transmitly/assets/3877248/34239edd-234d-4bee-9352-49d781716364" width="500" align="right">
</picture> 

---------------------------------------------------

_Copyright &copy; Code Impressions, LLC - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
