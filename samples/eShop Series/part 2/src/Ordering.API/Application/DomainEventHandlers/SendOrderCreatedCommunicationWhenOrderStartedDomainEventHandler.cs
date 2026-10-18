namespace eShop.Ordering.API.Application.DomainEventHandlers;

public class SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler
                    : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly ICommunicationsClient _communicationsClient;
    private readonly IIdentityService _identityService;
    private readonly ILogger _logger;

    public SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler(
        ICommunicationsClient communicationsClient,
        IIdentityService identityService,
        ILogger<SendOrderCreatedCommunicationWhenOrderStartedDomainEventHandler> logger)
    {
        _communicationsClient = communicationsClient ?? throw new ArgumentNullException(nameof(communicationsClient));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(OrderStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // The buyer's email comes from the access token on the create-order request.
        // Ordering doesn't store it; Communications decides what to do with it.
        var buyerEmail = _identityService.GetUserEmail();
        if (string.IsNullOrWhiteSpace(buyerEmail))
        {
            _logger.LogWarning(
                "No email claim was found for buyer {UserId}; the OrderCreated communication for order {OrderId} was not dispatched.",
                domainEvent.UserId,
                domainEvent.Order.Id);
            return;
        }

        // Ordering only states the intent and the facts it owns. The Communications
        // pipeline decides how, and whether, the buyer hears about it.
        await _communicationsClient.DispatchAsync(
            CommunicationIntents.OrderCreated,
            buyerEmail,
            new { orderId = domainEvent.Order.Id },
            cancellationToken: cancellationToken);
    }
}
