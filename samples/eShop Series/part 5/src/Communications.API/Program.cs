using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.Apis;
using eShop.Communications.API.DeliveryReports;
using eShop.Communications.API.OrderCreated;
using eShop.Communications.API.PushRegistrations;
using eShop.ServiceDefaults;
using eShop.ServiceDefaults.Communications;
using Transmitly;

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
    .AddOrderCreatedPipeline(webAppUrl, providers));

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
