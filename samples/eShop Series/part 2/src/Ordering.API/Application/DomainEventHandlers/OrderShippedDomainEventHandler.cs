namespace eShop.Ordering.API.Application.DomainEventHandlers;

public class OrderShippedDomainEventHandler
                : INotificationHandler<OrderShippedDomainEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
    private readonly ICommunicationsClient _communicationsClient;
    private readonly ILogger _logger;

    public OrderShippedDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<OrderShippedDomainEventHandler> logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService,
        ICommunicationsClient communicationsClient)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService;
        _communicationsClient = communicationsClient ?? throw new ArgumentNullException(nameof(communicationsClient));
    }

    public async Task Handle(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.Order.Id, OrderStatus.Shipped);

        //existing eShop convention of requiring composition of the order and buyer to create the integration event
        var order = await _orderRepository.GetAsync(domainEvent.Order.Id);
        var buyer = await _buyerRepository.FindByIdAsync(order.BuyerId.Value);

        var integrationEvent = new OrderStatusChangedToShippedIntegrationEvent(order.Id, order.OrderStatus, buyer.Name, buyer.IdentityGuid);
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);


        // if we were only Dispatching our intent we would never have to have to load the order and buyer objects
        // notice here we're only using the identifiers. Communications will be responsible for loading the data it needs to populate the communication template.
        // if we also had payment information with our order we could have passed the payment id and communications would have been responsible for loading
        // the payment information to populate the template.
        var recipient = new IdentityReference("Buyer", buyer.IdentityGuid);
        var model = TransactionModel.Create(new { orderId = order.Id });

        var result = await _communicationsClient.DispatchAsync(
            CommunicationIntents.OrderShipped,
            [recipient],
            model,
            cancellationToken: cancellationToken);

        if (!result.IsSuccessful)
        {
            _logger.LogWarning(
                "The OrderShipped communication for order {OrderId} was not dispatched successfully.",
                order.Id);
        }
    }
}
