using eShop.EventBus.Events;

namespace eShop.OrderProcessor.IntegrationEvents.Events;

public record OrderStatusChangedToPaidIntegrationEvent(int OrderId) : IntegrationEvent;
