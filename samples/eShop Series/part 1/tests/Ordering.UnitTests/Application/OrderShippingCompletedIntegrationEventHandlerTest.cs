using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Ordering.UnitTests.Application;

[TestClass]
public class OrderShippingCompletedIntegrationEventHandlerTest
{
    [TestMethod]
    public async Task SendsShipOrderCommandForCompletedShipment()
    {
        var mediator = Substitute.For<IMediator>();
        var logger = Substitute.For<ILogger<OrderShippingCompletedIntegrationEventHandler>>();
        var handler = new OrderShippingCompletedIntegrationEventHandler(mediator, logger);

        await handler.Handle(new OrderShippingCompletedIntegrationEvent(42));

        await mediator.Received(1).Send(
            Arg.Is<ShipOrderCommand>(command => command.OrderNumber == 42),
            Arg.Any<CancellationToken>());
    }
}
