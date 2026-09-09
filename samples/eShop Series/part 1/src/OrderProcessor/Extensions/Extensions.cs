using System.Text.Json.Serialization;
using eShop.OrderProcessor.Events;
using eShop.OrderProcessor.IntegrationEvents.EventHandling;
using eShop.OrderProcessor.IntegrationEvents.Events;

namespace eShop.OrderProcessor.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddRabbitMqEventBus("eventbus")
               .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>()
               .ConfigureJsonOptions(options => options.TypeInfoResolverChain.Add(IntegrationEventContext.Default));

        builder.AddNpgsqlDataSource("orderingdb");

        builder.Services.AddOptions<BackgroundTaskOptions>()
            .BindConfiguration(nameof(BackgroundTaskOptions));

        builder.Services.AddSingleton<IGracePeriodOrdersRepository, GracePeriodOrdersRepository>();
        builder.Services.AddHostedService<GracePeriodManagerService>();
    }
}

[JsonSerializable(typeof(GracePeriodConfirmedIntegrationEvent))]
[JsonSerializable(typeof(OrderStatusChangedToPaidIntegrationEvent))]
[JsonSerializable(typeof(OrderShippingCompletedIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext
{

}
