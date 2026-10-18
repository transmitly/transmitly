namespace eShop.Ordering.API.Application.DomainEventHandlers;

public class SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler
                    : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly ICommunicationsClient _communicationsClient;

    public SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler(
        ICommunicationsClient communicationsClient)
    {
        _communicationsClient = communicationsClient ?? throw new ArgumentNullException(nameof(communicationsClient));
    }

    public async Task Handle(OrderStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Ordering identifies the buyer by the identity it already owns. Communications
        // resolves that reference to the addresses it needs, so Ordering never handles them.
        var buyer = new IdentityReference(CommunicationIdentityTypes.Buyer, domainEvent.UserId);

        // The order aggregate is already in hand, so project the facts Ordering owns
        // instead of making Communications load them again.
        var order = domainEvent.Order;
        var model = new OrderCreatedModel(
            order.Id,
            order.OrderDate,
            order.OrderItems
                .Select(item => new OrderCreatedItem(
                    item.ProductId,
                    item.ProductName,
                    item.Units,
                    item.UnitPrice))
                .ToArray(),
            order.GetTotal());

        // Ordering only states the intent and the facts it owns. The Communications
        // pipeline decides how, and whether, the buyer hears about it.
        await _communicationsClient.DispatchAsync(
            CommunicationIntents.OrderCreated,
            [buyer],
            TransactionModel.Create(model),
            cancellationToken: cancellationToken);
    }
}
