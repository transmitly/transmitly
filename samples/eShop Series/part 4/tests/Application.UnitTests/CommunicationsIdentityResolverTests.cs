using System.Net;
using System.Net.Http.Json;
using System.Text;
using eShop.Communications.API;
using eShop.ServiceDefaults.Communications;
using eShop.ServiceDefaults.Identity;
using Microsoft.Extensions.DependencyInjection;
using Transmitly;

namespace eShop.Application.UnitTests;

[TestClass]
public class CommunicationsIdentityResolverTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task TransmitlyResolvesIdentityUsingDiConfiguredHttpClient()
    {
        var identityServer = new IdentityServerHandler();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<IdentityServerCustomerIdentityResolver>(client =>
                client.BaseAddress = new Uri("http://identity-api"))
            .ConfigurePrimaryHttpMessageHandler(() => identityServer);

        services.AddTransmitly(tly => tly
            .AddSimulationSupport()
            .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
            .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
            {
                pipeline.AddEmail(
                    "orders@eshop.local".AsIdentityAddress("eShop"),
                    email => email.Subject.AddStringTemplate("Thanks for your order!"));
            }));

        await using var serviceProvider = services.BuildServiceProvider();
        var communicationsClient = serviceProvider.GetRequiredService<ICommunicationsClient>();

        var result = await communicationsClient.DispatchAsync(
            CommunicationIntents.OrderCreated,
            [new IdentityReference(CommunicationIdentityTypes.Buyer, "buyer-123")],
            TransactionModel.Create(new { orderId = 42 }),
            [],
            cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccessful);
        CollectionAssert.AreEqual(
            new[]
            {
                "http://identity-api/connect/token",
                "http://identity-api/api/identities/resolve"
            },
            identityServer.RequestUris);
        Assert.AreEqual("Bearer test-token", identityServer.IdentityRequestAuthorization);
    }

    private sealed class IdentityServerHandler : HttpMessageHandler
    {
        public List<string> RequestUris { get; } = [];
        public string? IdentityRequestAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!.ToString());

            if (request.RequestUri.AbsolutePath == "/connect/token")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"access_token\":\"test-token\"}",
                        Encoding.UTF8,
                        "application/json")
                });
            }

            IdentityRequestAuthorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<IReadOnlyCollection<ResolvedIdentityProfile>>(
                [
                    new ResolvedIdentityProfile(
                        "buyer-123",
                        "alice",
                        "Alice",
                        "Smith",
                        "Alice Smith",
                        [new ResolvedIdentityAddress(
                            "alice@example.com",
                            "Alice Smith",
                            "Email",
                            true)])
                ])
            });
        }
    }
}
