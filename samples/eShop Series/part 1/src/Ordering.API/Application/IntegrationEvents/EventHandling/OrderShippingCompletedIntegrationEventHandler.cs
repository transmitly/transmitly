namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling;

public sealed class OrderShippingCompletedIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderShippingCompletedIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderShippingCompletedIntegrationEvent>
{
    public async Task Handle(OrderShippingCompletedIntegrationEvent @event)
    {
        logger.LogInformation(
            "Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})",
            @event.Id,
            @event);

        var command = new ShipOrderCommand(@event.OrderId);

        logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.OrderNumber),
            command.OrderNumber,
            command);

        await mediator.Send(command);
    }
}
