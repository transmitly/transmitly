#nullable enable

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.OrderCreated;
using eShop.Communications.API.PushRegistrations;
using eShop.ServiceDefaults.Communications;
using eShop.ServiceDefaults.Identity;
using Microsoft.Extensions.DependencyInjection;
using Transmitly;
using Transmitly.Delivery;

namespace eShop.Application.UnitTests;

[TestClass]
public class OrderCreatedCompositionTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly OrderCreatedModel Order = new(
        123,
        new DateTime(2026, 10, 1, 16, 4, 12, DateTimeKind.Utc),
        [
            new OrderCreatedItem(1, ".NET Bot Black Hoodie", 2, 42.00m),
            new OrderCreatedItem(2, ".NET Mug", 1, 14.99m)
        ],
        98.99m);

    [TestMethod]
    public async Task OrderCreatedIsSentOnEveryChannelThroughItsOwnProvider()
    {
        var dispatch = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: true));

        // One dispatch per channel: no channel goes out through more than one provider.
        Assert.HasCount(3, dispatch.Result.Results);
        Assert.AreEqual(EshopChannelProviders.Email, dispatch.Report<IEmail>().ChannelProviderId);
        Assert.AreEqual(EshopChannelProviders.Sms, dispatch.Report<ISms>().ChannelProviderId);
        Assert.AreEqual(EshopChannelProviders.Push, dispatch.Report<IPushNotification>().ChannelProviderId);
    }

    [TestMethod]
    public async Task EmailIsComposedFromOrderingIdentityAndCatalog()
    {
        var dispatch = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: true));
        var email = dispatch.Communication<IEmail>();
        TestContext.WriteLine($"Subject: {email.Subject}{Environment.NewLine}{email.TextBody}");

        Assert.AreEqual("Thanks for your order #123", email.Subject);
        StringAssert.Contains(email.TextBody, "Hi Alice,");
        StringAssert.Contains(email.TextBody, "2 × .NET Bot Black Hoodie");
        StringAssert.Contains(email.TextBody, "AdventureWorks Apparel");
        // The purchased price comes from the order, not Catalog's current price.
        StringAssert.Contains(email.TextBody, "$42.00 each");
        StringAssert.Contains(email.TextBody, "1 × .NET Mug");
        StringAssert.Contains(email.TextBody, "Order total: $98.99");
        StringAssert.Contains(email.TextBody, "https://eshop.test/user/orders");
    }

    [TestMethod]
    public async Task SmsIsShortAndGoesToThePhoneNumberFromIdentity()
    {
        var dispatch = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: true));
        var sms = dispatch.Communication<ISms>();
        TestContext.WriteLine(sms.Message);

        Assert.AreEqual("1234567890", sms.To!.Single().Value);
        Assert.AreEqual(
            "We've received your eShop order #123.\nView your order: https://eshop.test/user/orders",
            sms.Message);
    }

    [TestMethod]
    public async Task PushGoesToTheRegisteredDeviceAndCarriesTheOrder()
    {
        var dispatch = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: true));
        var push = dispatch.Communication<IPushNotification>();

        Assert.AreEqual("simulated-device-buyer-123", push.Recipient.Single().Value);
        Assert.AreEqual("Thanks for your order", push.Title);
        Assert.AreEqual("Order #123 has been received.", push.Body);
        Assert.AreEqual(OrderCreatedPush.OpenOrderAction, push.Data![OrderCreatedPush.ActionKey]);
        Assert.AreEqual("123", push.Data[OrderCreatedPush.OrderIdKey]);
    }

    [TestMethod]
    public async Task EmailIsStillSentWhenCatalogIsUnavailable()
    {
        var dispatch = await DispatchOrderCreatedAsync(new EshopServicesHandler(catalogAvailable: false));
        var email = dispatch.Communication<IEmail>();

        Assert.AreEqual("Thanks for your order #123", email.Subject);
        StringAssert.Contains(email.TextBody, "2 × .NET Bot Black Hoodie");
        Assert.IsFalse(email.TextBody!.Contains("AdventureWorks"));
    }

    private async Task<Dispatch> DispatchOrderCreatedAsync(EshopServicesHandler handler)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<IdentityServerCustomerIdentityResolver>(client =>
                client.BaseAddress = new Uri("http://identity-api"))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddHttpClient<CatalogContentModelEnricher>(client =>
                client.BaseAddress = new Uri("http://catalog-api"))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddSimulatedPushRegistrations();

        // The simulation providers raise delivery reports without awaiting them.
        var reports = new ConcurrentQueue<DeliveryReport>();
        var allChannelsReported = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        services.AddTransmitly(tly => tly
            .AddEshopChannelProviders()
            .AddDeliveryReportHandler(report =>
            {
                reports.Enqueue(report);
                if (reports.Count >= 3)
                {
                    allChannelsReported.TrySetResult();
                }

                return Task.CompletedTask;
            })
            .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
            .AddPlatformIdentityProfileEnricher<PushRegistrationProfileEnricher>(CommunicationIdentityTypes.Buyer)
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

        Assert.IsTrue(
            result.IsSuccessful,
            string.Join("; ", result.Results.Select(r => $"{r?.ChannelId}/{r?.ChannelProviderId}: {r?.Status} {r?.Exception?.Message}")));
        await allChannelsReported.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
        return new Dispatch(result, [.. reports]);
    }

    private sealed record Dispatch(IDispatchCommunicationResult Result, IReadOnlyCollection<DeliveryReport> Reports)
    {
        public DeliveryReport Report<T>() => Reports.Single(report => report.ChannelCommunication is T);

        public T Communication<T>() => (T)Report<T>().ChannelCommunication!;
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
                            [
                                new ResolvedIdentityAddress("alice@example.com", "Alice Smith", "email", true),
                                new ResolvedIdentityAddress("1234567890", "Alice Smith", "phone", false)
                            ])
                    ]),
                "catalog-api" when !catalogAvailable =>
                    new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                "catalog-api" when uri.AbsolutePath == "/api/catalog/items/by" =>
                    Json(new[]
                    {
                        // Catalog's current price differs from what the customer paid.
                        new { id = 1, name = ".NET Bot Black Hoodie", description = "Hoodie", price = 50.00m, catalogTypeId = 2, catalogBrandId = 2 },
                        new { id = 2, name = ".NET Mug", description = "Mug", price = 14.99m, catalogTypeId = 1, catalogBrandId = 1 }
                    }),
                "catalog-api" when uri.AbsolutePath == "/api/catalog/catalogbrands" =>
                    Json(new[] { new { id = 1, brand = "AdventureWorks" }, new { id = 2, brand = "AdventureWorks Apparel" } }),
                "catalog-api" when uri.AbsolutePath == "/api/catalog/catalogtypes" =>
                    Json(new[] { new { id = 1, type = "Mug" }, new { id = 2, type = "T-Shirt" } }),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            });
        }

        private static HttpResponseMessage Json<T>(T value) =>
            new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };
    }
}
