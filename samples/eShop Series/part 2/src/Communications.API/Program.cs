using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.Apis;
using eShop.ServiceDefaults;
using eShop.ServiceDefaults.Communications;
using Transmitly;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.AddFilter("Transmitly", Microsoft.Extensions.Logging.LogLevel.Debug);
builder.Services.AddProblemDetails();

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
    .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
    {
        pipeline.AddEmail(
            "orders@eshop.local".AsIdentityAddress("eShop"),
            email =>
            {
                email.Subject.AddStringTemplate("Thanks for your order!");
                email.TextBody.AddStringTemplate(
                    "We've received your order and we'll let you know when it ships.");
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
