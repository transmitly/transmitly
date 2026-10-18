#nullable enable

using eShop.Ordering.API.Infrastructure.Communications;
using eShop.ServiceDefaults.Communications;
using Microsoft.Extensions.Logging.Abstractions;
using Transmitly;

namespace eShop.Ordering.UnitTests.Application;

[TestClass]
public class TransactionalCommunicationsMiddlewareTest
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task DispatchInsideTransactionIsSentOnlyAfterFlush()
    {
        var (inner, client) = CreateClient();

        using (var pending = PendingCommunications.Begin())
        {
            var result = await DispatchOrderCreatedAsync(client);

            Assert.IsTrue(result.IsSuccessful);
            AssertDispatchCount(inner, 0);

            await pending.FlushAsync(NullLogger.Instance, TestContext.CancellationToken);
        }

        AssertDispatchCount(inner, 1);
    }

    [TestMethod]
    public async Task DispatchInsideTransactionIsDroppedWhenNotFlushed()
    {
        var (inner, client) = CreateClient();

        using (PendingCommunications.Begin())
        {
            await DispatchOrderCreatedAsync(client);
        }

        AssertDispatchCount(inner, 0);
    }

    [TestMethod]
    public async Task DispatchOutsideTransactionIsSentImmediately()
    {
        var (inner, client) = CreateClient();

        await DispatchOrderCreatedAsync(client);

        AssertDispatchCount(inner, 1);
    }

    private static (ICommunicationsClient Inner, ICommunicationsClient Client) CreateClient()
    {
        var inner = Substitute.For<ICommunicationsClient>();
        var client = new TransactionalCommunicationsMiddleware().CreateClient(
            Substitute.For<ICreateCommunicationsClientContext>(),
            inner)!;

        return (inner, client);
    }

    private Task<IDispatchCommunicationResult> DispatchOrderCreatedAsync(ICommunicationsClient client) =>
        client.DispatchAsync(
            CommunicationIntents.OrderCreated,
            "alice@example.com",
            new { orderId = 42 },
            cancellationToken: TestContext.CancellationToken);

    private static void AssertDispatchCount(ICommunicationsClient inner, int count) =>
        _ = inner.Received(count).DispatchAsync(
            CommunicationIntents.OrderCreated,
            Arg.Any<IReadOnlyCollection<IPlatformIdentityProfile>>(),
            Arg.Any<ITransactionModel>(),
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
}
