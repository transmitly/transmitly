namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

public record OrderShippingCompletedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; }

    public OrderShippingCompletedIntegrationEvent(int orderId) => OrderId = orderId;
}
