# Hello Transmitly
<img alt="robot waving" src="https://github.com/user-attachments/assets/27f378e6-0aed-4ebf-87d7-8204cef97663" style="width:200px; height:150px;max-width:100%" align="right"/> 

This console application defines a single `WelcomeKit` communication pipeline and dispatches it using Transmitly's built-in simulation provider.



    

## What this sample demonstrates

The application defines a communication intent:

```csharp
"WelcomeKit"
```

That intent is configured to send an email with a static subject and body:

```csharp
pipeline.AddEmail(
    "welcome@example.com".AsIdentityAddress("Welcome"),
    email =>
    {
        email.Subject.AddStringTemplate("Welcome!");
        email.TextBody.AddStringTemplate(
            "Thanks for trying Transmitly.");
    });
```

The application itself only needs to express the intent and recipient:

```csharp
await communicationsClient.DispatchAsync(
    "WelcomeKit",
    "developer@example.com",
    new { });
```

Transmitly handles the configured communication pipeline behind that intent.

## Run the sample

From the sample directory:

```shell
dotnet run
```

You'll be prompted to dispatch the communication:

```text
Press any key to dispatch the communication...
```

Press a key and the simulated communication will be dispatched.

You should then see:

```text
Communication dispatched successfully.
```

Because this sample uses:

```csharp
.AddSimulationSupport()
```

nothing is actually sent to `developer@example.com`.

## Why simulation?

Simulation lets you configure and exercise Transmitly without first choosing or configuring an external provider.

Once you're ready to deliver real communications, the same pipeline model can be used with providers such as SMTP, SendGrid, Twilio, Infobip, Firebase, and others.

The important part is that your application can continue expressing the same intent:

```csharp
await communicationsClient.DispatchAsync(
    "WelcomeKit",
    recipient,
    model);
```

while the delivery configuration evolves separately.

## Next steps

Once this example makes sense, explore the other [Transmitly samples](https://github.com/transmitly/transmitly/tree/main/samples) for provider integrations, multiple channels, dependency injection, templates, delivery reports, and more.

For the full project documentation, visit the [Transmitly repository](https://github.com/transmitly/transmitly).
