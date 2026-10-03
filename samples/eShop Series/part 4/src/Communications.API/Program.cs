using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.Apis;
using eShop.Communications.API.OrderCreated;
using eShop.Communications.API.PushRegistrations;
using eShop.ServiceDefaults;
using eShop.ServiceDefaults.Communications;
using Transmitly;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.AddFilter("Transmitly", Microsoft.Extensions.Logging.LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient<IdentityServerCustomerIdentityResolver>(client =>
    client.BaseAddress = new Uri("https+http://identity-api"));
builder.Services.AddHttpClient<CatalogContentModelEnricher>(client =>
        client.BaseAddress = new Uri("https+http://catalog-api"))
    .AddApiVersion(1.0);
builder.Services.AddSimulatedPushRegistrations();

var webAppUrl = builder.Configuration["WebAppUrl"] is { Length: > 0 } url ? new Uri(url) : null;

var transmitlyLoggerFactory = new MicrosoftTransmitlyLoggerFactory();
ILogger? logger = null;

builder.Services.AddTransmitly(tly => tly
    .AddLogging(logging =>
    {
        logging.MinimumLevel = Transmitly.Logging.LogLevel.Debug;
        logging.LoggerFactory = transmitlyLoggerFactory;
    })
    .AddDeliveryReportHandler((report) =>
    {
        logger?.LogInformation("[{channelId}:{channelProviderId}:Dispatched] Id={id}; Content={communication}", report.ChannelId, report.ChannelProviderId, report.ResourceId, JsonSerializer.Serialize(report.ChannelCommunication, new JsonSerializerOptions { WriteIndented = true }));
        return Task.CompletedTask;
    })
    .AddEshopChannelProviders()
    .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
    .AddPlatformIdentityProfileEnricher<PushRegistrationProfileEnricher>(CommunicationIdentityTypes.Buyer)
    .AddOrderCreatedPipeline(webAppUrl));

var app = builder.Build();
logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Transmitly");
transmitlyLoggerFactory.UseLoggerFactory(
    app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapCommunicationsApi();

app.Run();
