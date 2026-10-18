#nullable enable

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using eShop.ServiceDefaults.Communications;
using Microsoft.Extensions.Hosting;
using Transmitly;

namespace eShop.Ordering.UnitTests.Application;

[TestClass]
public class EshopCommunicationsMiddlewareTest
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task DispatchForwardsAddressesAndMetadataToCommunicationsService()
    {
        var handler = new SuccessfulDispatchHandler();
        using var httpClient = new HttpClient(handler);
        var environment = Substitute.For<IHostEnvironment>();
        environment.ApplicationName.Returns("Ordering.API");

        var middleware = new EshopCommunicationsMiddleware(httpClient, environment);
        var client = middleware.CreateClient(
            Substitute.For<ICreateCommunicationsClientContext>(),
            previous: null);

        using var activity = new Activity("create-order").Start();
        activity.AddBaggage("tenant", "contoso");

        var result = await client.DispatchAsync(
            CommunicationIntents.OrderCreated,
            "alice@example.com",
            new { orderId = 42 },
            cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(
            new Uri("http://communications-api/api/communications/dispatch"),
            handler.RequestUri);

        var request = handler.Request!;
        Assert.AreEqual(CommunicationIntents.OrderCreated, request.Intent);
        Assert.AreEqual("alice@example.com", request.Recipients.Single().Addresses.Single().Value);
        Assert.AreEqual(42, request.Model.GetProperty("orderId").GetInt32());
        Assert.AreEqual("Ordering.API", request.Metadata.Source);
        Assert.AreEqual(activity.TraceId.ToHexString(), request.Metadata.CorrelationId);
        Assert.AreEqual("contoso", request.Metadata.Properties["tenant"]);
    }

    private sealed class SuccessfulDispatchHandler : HttpMessageHandler
    {
        public Uri RequestUri { get; private set; } = null!;
        public CommunicationDispatchRequest? Request { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri!;
            Request = await request.Content!.ReadFromJsonAsync<CommunicationDispatchRequest>(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new CommunicationDispatchResponse(
                    true,
                    1,
                    Request!.Metadata.CorrelationId))
            };
        }
    }
}
