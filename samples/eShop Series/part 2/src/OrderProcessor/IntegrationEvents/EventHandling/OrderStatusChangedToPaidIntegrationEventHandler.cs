using eShop.EventBus.Abstractions;
using eShop.OrderProcessor.IntegrationEvents.Events;

namespace eShop.OrderProcessor.IntegrationEvents.EventHandling;

public sealed class OrderStatusChangedToPaidIntegrationEventHandler(
    IEventBus eventBus,
    ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        logger.LogInformation(
            "Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})",
            @event.Id,
            @event);

        // Simulate the fulfillment system completing shipment. Keeping this as
        // an integration event lets Ordering remain responsible for its state.
        var shippingCompleted = new OrderShippingCompletedIntegrationEvent(@event.OrderId);

        logger.LogInformation(
            "Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})",
            shippingCompleted.Id,
            shippingCompleted);

        await eventBus.PublishAsync(shippingCompleted);
    }
}
