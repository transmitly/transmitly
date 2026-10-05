#nullable enable

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.OrderCreated;
using eShop.ServiceDefaults.Communications;
using eShop.ServiceDefaults.Identity;
using Microsoft.Extensions.DependencyInjection;
using Transmitly;

namespace eShop.Application.UnitTests;

[TestClass]
public class OrderCreatedCompositionTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly OrderCreatedModel Order = new(
        123,
        new DateTime(2026, 10, 1, 16, 4, 12, DateTimeKind.Utc),
        [
            new OrderCreatedItem(1, "Wanderer Black Hiking Boots", 2, 109.99m),
            new OrderCreatedItem(2, "Summit Pro Harness", 1, 89.99m)
        ],
        309.97m);

    [TestMethod]
    public async Task EmailIsComposedFromOrderingIdentityAndCatalog()
    {
        var email = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: true));
        TestContext.WriteLine($"Subject: {email.Subject}{Environment.NewLine}{email.TextBody}");

        Assert.AreEqual("Thanks for your order #123", email.Subject);
        StringAssert.Contains(email.TextBody, "Hi Alice,");
        StringAssert.Contains(email.TextBody, "2 × Wanderer Black Hiking Boots");
        StringAssert.Contains(email.TextBody, "Daybird");
        // The purchased price comes from the order, not Catalog's current price.
        StringAssert.Contains(email.TextBody, "$109.99 each");
        StringAssert.Contains(email.TextBody, "1 × Summit Pro Harness");
        StringAssert.Contains(email.TextBody, "Order total: $309.97");
        StringAssert.Contains(email.TextBody, "https://eshop.test/user/orders");
    }

    [TestMethod]
    public async Task EmailIsStillSentWhenCatalogIsUnavailable()
    {
        var email = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: false));

        Assert.AreEqual("Thanks for your order #123", email.Subject);
        StringAssert.Contains(email.TextBody, "2 × Wanderer Black Hiking Boots");
        Assert.IsFalse(email.TextBody!.Contains("Daybird"));
    }

    private async Task<IEmail> DispatchOrderCreatedAsync(EshopServicesHandler handler)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<IdentityServerCustomerIdentityResolver>(client =>
                client.BaseAddress = new Uri("http://identity-api"))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddHttpClient<CatalogContentModelEnricher>(client =>
                client.BaseAddress = new Uri("http://catalog-api"))
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        // The simulation provider raises delivery reports without awaiting them.
        var sent = new TaskCompletionSource<IEmail>(TaskCreationOptions.RunContinuationsAsynchronously);
        services.AddTransmitly(tly => tly
            .AddSimulationSupport()
            .AddDeliveryReportHandler(report =>
            {
                if (report.ChannelCommunication is IEmail email)
                {
                    sent.TrySetResult(email);
                }

                return Task.CompletedTask;
            })
            .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
            .AddOrderCreatedPipeline(new Uri("https://eshop.test/")));

        await using var serviceProvider = services.BuildServiceProvider();
        var communicationsClient = serviceProvider.GetRequiredService<ICommunicationsClient>();

        // Mirror the hop between Ordering and Communications: the model travels as JSON
        // and the dispatch endpoint converts it before handing it to Transmitly.
        var model = TransactionModelJson.ToModel(JsonSerializer.SerializeToElement(Order))!;

        var result = await communicationsClient.DispatchAsync(
            CommunicationIntents.OrderCreated,
            [new IdentityReference(CommunicationIdentityTypes.Buyer, "buyer-123")],
            TransactionModel.Create(model),
            [],
            cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccessful);
        return await sent.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
    }

    private sealed class EshopServicesHandler(bool catalogAvailable) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;

            return Task.FromResult(uri.Host switch
            {
                "identity-api" when uri.AbsolutePath == "/connect/token" =>
                    Json(new { access_token = "test-token" }),
                "identity-api" =>
                    Json<IReadOnlyCollection<ResolvedIdentityProfile>>(
                    [
                        new ResolvedIdentityProfile(
                            "buyer-123",
                            "alice",
                            "Alice",
                            "Smith",
                            "Alice Smith",
                            [new ResolvedIdentityAddress("alice@example.com", "Alice Smith", "email", true)])
                    ]),
                "catalog-api" when !catalogAvailable =>
                    new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                "catalog-api" when uri.AbsolutePath == "/api/catalog/items/by" =>
                    Json(new[]
                    {
                        // Catalog's current price differs from what the customer paid.
                        new { id = 1, name = "Wanderer Black Hiking Boots", description = "Waterproof leather hiking boots", price = 119.99m, catalogTypeId = 1, catalogBrandId = 1 },
                        new { id = 2, name = "Summit Pro Harness", description = "Lightweight climbing harness", price = 89.99m, catalogTypeId = 2, catalogBrandId = 2 }
                    }),
                "catalog-api" when uri.AbsolutePath == "/api/catalog/catalogbrands" =>
                    Json(new[] { new { id = 1, brand = "Daybird" }, new { id = 2, brand = "Gravitator" } }),
                "catalog-api" when uri.AbsolutePath == "/api/catalog/catalogtypes" =>
                    Json(new[] { new { id = 1, type = "Footwear" }, new { id = 2, type = "Climbing" } }),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            });
        }

        private static HttpResponseMessage Json<T>(T value) =>
            new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };
    }
}
