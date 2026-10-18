using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.Apis;
using eShop.Communications.API.OrderCreated;
using eShop.ServiceDefaults;
using eShop.ServiceDefaults.Communications;
using Transmitly;
using Transmitly.Model.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.AddFilter("Transmitly", Microsoft.Extensions.Logging.LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient<IdentityServerCustomerIdentityResolver>(client =>
    client.BaseAddress = new Uri("https+http://identity-api"));
builder.Services.AddHttpClient<CatalogContentModelEnricher>(client =>
        client.BaseAddress = new Uri("https+http://catalog-api"))
    .AddApiVersion(1.0);

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
    .AddSimulationSupport()
    .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
    .AddContentModelEnricher<CatalogContentModelEnricher>(options =>
    {
        // Enrich once per recipient so every channel shares the same content.
        options.Scope = ContentModelEnricherScope.PerRecipient;
        options.Predicate = context => context.PipelineIntent == CommunicationIntents.OrderCreated;
    })
    .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
    {
        pipeline.AddEmail(
            "orders@eshop.local".AsIdentityAddress("eShop"),
            email =>
            {
                email.Subject.AddTemplateResolver(context => OrderCreatedEmail.Subject(context));
                email.TextBody.AddTemplateResolver(context => OrderCreatedEmail.TextBody(context, webAppUrl));
            });
    }));

var app = builder.Build();
logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Transmitly");
transmitlyLoggerFactory.UseLoggerFactory(
    app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapCommunicationsApi();

app.Run();
