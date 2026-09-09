using eShop.EventBus.Events;

namespace eShop.OrderProcessor.IntegrationEvents.Events;

public record OrderShippingCompletedIntegrationEvent(int OrderId) : IntegrationEvent;
