# Communications, done right
Transmitly is a powerful and vendor-agnostic communication library designed to simplify and enhance the process of sending transactional messages across various platforms. With its easy-to-use API, developers can seamlessly integrate email, SMS, and other messaging services into their applications, ensuring reliable and efficient delivery of critical notifications. Built for flexibility and scalability, Transmitly supports multiple communication channels, allowing you to focus on building great applications while it handles the complexity of message transmission.

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

Using Microsoft Dependency Injection? Give the [Transmitly MS DI extension](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection) a go instead of using the builder directly.

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
//Pipelines are the heart of Transmitly. Pipelines allow you to define your communications
//as a domain action. This allows your domain code to stay agnostic to the details of how you
//may send out a transactional communication.
.AddPipeline("WelcomeKit", pipeline =>
{
    //AddEmail is a channel that is core to the Transmitly library.
    //AsIdentityAddress() is also a convenience method that helps us create an audience identity
    //Identity addresses can be anything, email, phone, or even a device/app Id for push notifications!
    pipeline.AddEmail("welcome@my.app".AsIdentityAddress("Welcome Committee"), email =>
    {
       //Transmitly is a bit different. All of our content is supported by templates out of the box.
       //There are multiple types of templates to get you started. You can even create templates 
       //specific to certain cultures! For this example we'll keep things simple and send a static message.
       email.Subject.AddStringTemplate("Thanks for creating an account!");
       email.HtmlBody.AddStringTemplate("Check out the <a href=\"https://my.app/getting-started\">Getting Started</a> section to see all the cool things you can do!");
       email.TextBody.AddStringTemplate("Check out the Getting Started (https://my.app/getting-started) section to see all the cool things you can do!");
    });
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
    var result = await _communicationsClient.DispatchAsync("WelcomeKit", "newAccount@gmail.com", new{});

    if(result.IsSuccessful)
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



### Changing Channel Providers
Normally, changing from SMTP with MailKit can be somewhat of an undertaking. With Transmitly it's as easy as adding a your prefered channel provider and configuring. Your channel configuration stays the same. Your domain code stays the same. And things work as you expect.

For the next example we'll start using SendGrid to send our emails. 
```shell
dotnet install Transmitly.ChannelProvider.Sendgrid
```

Next we'll update our configuration. Notice we've removed MailKitSupport and added SendGridSupport. 
```csharp
using Transmitly;

//CommunicationsClientBuilder is a fluent way to configure our communication settings and pipline
ICommunicationsClient communicationsClient = new CommunicationsClientBuilder()
//Transmitly.ChannelProvider.MailKit adds on to the client builder with it's own extensions to make adding setup a breeze
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
//Pipelines are the heart of Transmitly. Pipelines allow you to define your communications
//as a domain action. This allows your domain code to stay agnostic to the details of how you
//may send out a transactional communication.
.AddPipeline("WelcomeKit", pipeline =>
{
    //AddEmail is a channel that is core to the Transmitly library.
    //AsIdentityAddress() is also a convenience method that helps us create an audience identity
    //Identity addresses can be anything, email, phone, or even a device/app Id for push notifications!
    pipeline.AddEmail("welcome@my.app".AsIdentityAddress("Welcome Committee"), email =>
    {
       //Transmitly is a bit different. All of our content is supported by templates out of the box.
       //There are multiple types of templates to get you started. You can even create templates 
       //specific to certain cultures! For this example we'll keep things simple and send a static message.
       email.Subject.AddStringTemplate("Thanks for creating an account!");
       email.HtmlBody.AddStringTemplate("Check out the <a href=\"https://my.app/getting-started\">Getting Started</a> section to see all the cool things you can do!");
       email.TextBody.AddStringTemplate("Check out the Getting Started (https://my.app/getting-started) section to see all the cool things you can do!");
    });
//We're done configuring, now we need to create our new communications client
.BuildClient();

//In this case, we're using Microsoft.DependencyInjection. We need to register our `ICommunicationsClient` with the service collection
builder.Services.AddSingleton(communicationsClient);
```

### Next Steps
We've only scratched the surface. Transmitly can do a **LOT** more to _deliver_ more value for your entire team. [Check out the wiki to learn](https://github.com/transmitly/transmitly/wiki) more about Transmitly's concepts as well as check out our examples to help you get started quickly.


<picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://github.com/transmitly/transmitly/assets/3877248/524f26c8-f670-4dfa-be78-badda0f48bfb">
  <img alt="an open-source project sponsored by CiLabs of Code Impressions, LLC" src="https://github.com/transmitly/transmitly/assets/3877248/34239edd-234d-4bee-9352-49d781716364" width="500" align="right">
</picture> 

---------------------------------------------------

_Copyright &copy; Code Impressions, LLC - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._
