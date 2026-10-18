using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.Apis;
using eShop.Communications.API.DeliveryReports;
using eShop.Communications.API.OrderCreated;
using eShop.Communications.API.PushRegistrations;
using eShop.ServiceDefaults;
using eShop.ServiceDefaults.Communications;
using Transmitly;
using Transmitly.Model.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultAuthentication();
builder.Logging.AddFilter("Transmitly", Microsoft.Extensions.Logging.LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient<IdentityServerCustomerIdentityResolver>(client =>
    client.BaseAddress = new Uri("https+http://identity-api"));
builder.Services.AddHttpClient<CatalogContentModelEnricher>(client =>
        client.BaseAddress = new Uri("https+http://catalog-api"))
    .AddApiVersion(1.0);
builder.Services.AddSimulatedPushRegistrations();

// Communications' own database for delivery history.
builder.AddNpgsqlDbContext<CommunicationsContext>("communicationsdb");
builder.Services.AddMigration<CommunicationsContext>();
builder.Services.AddDeliveryReportRecording(out var deliveryReports);

// Provider webhooks arrive here. Each provider's adaptor turns its requests into delivery reports.
builder.Services.AddControllers(options => options.AddTransmitlyDeliveryReportModelBinders());

var webAppUrl = builder.Configuration["WebAppUrl"] is { Length: > 0 } url ? new Uri(url) : null;
var providers = EshopChannelProviders.FromConfiguration(builder.Configuration);

var transmitlyLoggerFactory = new MicrosoftTransmitlyLoggerFactory();
ILogger? logger = null;

builder.Services.AddTransmitly(tly => providers.Register(tly
    .AddLogging(logging =>
    {
        logging.MinimumLevel = Transmitly.Logging.LogLevel.Debug;
        logging.LoggerFactory = transmitlyLoggerFactory;
    })
    .AddDeliveryReportHandler((report) =>
    {
        logger?.LogInformation("[{channelId}:{channelProviderId}:{eventName}] Id={id}; Content={communication}", report.ChannelId, report.ChannelProviderId, report.EventName, report.ResourceId, JsonSerializer.Serialize(report.ChannelCommunication, new JsonSerializerOptions { WriteIndented = true }));
        return Task.CompletedTask;
    })
    // Every report, from any provider, reaches the same recorder.
    .AddDeliveryReportHandler(report =>
    {
        deliveryReports.Enqueue(report);
        return Task.CompletedTask;
    }))
    .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
    .AddPlatformIdentityProfileEnricher<PushRegistrationProfileEnricher>(CommunicationIdentityTypes.Buyer)
    .AddContentModelEnricher<CatalogContentModelEnricher>(options =>
    {
        // Enrich once per recipient so every channel shares the same content.
        options.Scope = ContentModelEnricherScope.PerRecipient;
        options.Predicate = context => context.PipelineIntent == CommunicationIntents.OrderCreated;
    })
    .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
    {
        // The default first-match strategy: each buyer gets one message, on the first
        // channel below that one of their resolved addresses can use.
        pipeline.AddEmail(
            "orders@eshop.local".AsIdentityAddress("eShop"),
            email =>
            {
                email.AddChannelProviderFilter(providers.Email);
                email.Subject.AddTemplateResolver(context => OrderCreatedEmail.Subject(context));
                email.TextBody.AddTemplateResolver(context => OrderCreatedEmail.TextBody(context, webAppUrl));
            });

        pipeline.AddSms(
            providers.SmsFromNumber.AsIdentityAddress("eShop"),
            sms =>
            {
                sms.AddChannelProviderFilter(providers.Sms);

                // Where Twilio posts status updates. Transmitly adds the routing details
                // the webhook needs to recognize each update as a Twilio SMS report.
                sms.Twilio().StatusCallbackUrl = providers.DeliveryReportUrl;

                sms.Message.AddTemplateResolver(context => OrderCreatedSms.Message(context, webAppUrl));
            });

        pipeline.AddPushNotification(push =>
        {
            push.AddChannelProviderFilter(providers.Push);
            push.Title.AddTemplateResolver(context => OrderCreatedPush.Title(context));
            push.Body.AddTemplateResolver(context => OrderCreatedPush.Body(context));
            push.AddData(OrderCreatedPush.ActionKey, OrderCreatedPush.OpenOrderAction);
            push.AddDataIfNotNull(OrderCreatedPush.OrderIdKey, context => OrderCreatedPush.OrderId(context));
        });
    }));

var app = builder.Build();
logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Transmitly");
transmitlyLoggerFactory.UseLoggerFactory(
    app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapCommunicationsApi();
app.MapInboxApi();
app.MapControllers();

app.Run();
