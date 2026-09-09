using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using eShop.ServiceDefaults.Communications;
using Transmitly;

namespace eShop.Ordering.UnitTests.Application;

[TestClass]
public class EshopCommunicationsMiddlewareTest
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task DispatchUsesAbsoluteServiceDiscoveryUriWithoutBaseAddress()
    {
        var handler = new SuccessfulDispatchHandler();
        using var httpClient = new HttpClient(handler);
        var middleware = new EshopCommunicationsMiddleware(httpClient);
        var client = middleware.CreateClient(
            Substitute.For<ICreateCommunicationsClientContext>(),
            previous: null);

        var result = await client.DispatchAsync(
            CommunicationIntents.OrderShipped,
            [new IdentityReference("Buyer", "buyer-123")],
            TransactionModel.Create(new { orderId = 42 }),
            [],
            cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(
            new Uri("http://communications-api/api/communications/dispatch"),
            handler.RequestUri);
    }

    private sealed class SuccessfulDispatchHandler : HttpMessageHandler
    {
        public Uri RequestUri { get; private set; } = null!;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new CommunicationDispatchResponse(
                    true,
                    1,
                    "test-correlation"))
            });
        }
    }
}
