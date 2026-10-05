#nullable enable

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eShop.Communications.API;
using eShop.Communications.API.DeliveryReports;
using Microsoft.EntityFrameworkCore;
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
            new OrderCreatedItem(1, "Wanderer Black Hiking Boots", 2, 109.99m),
            new OrderCreatedItem(2, "Summit Pro Harness", 1, 89.99m)
        ],
        309.97m);

    private static readonly EshopChannelProviders Providers = EshopChannelProviders.Simulated();

    // Alice has verified her email address. Her phone number isn't verified.
    private static readonly ResolvedIdentityProfile Alice = new(
        "alice-id",
        "alice",
        "Alice",
        "Smith",
        "Alice Smith",
        [
            new ResolvedIdentityAddress("alice@example.com", "Alice Smith", "email", true),
            new ResolvedIdentityAddress("1234567890", "Alice Smith", "phone", false)
        ]);

    // Bob has verified his phone number. His email address isn't verified.
    private static readonly ResolvedIdentityProfile Bob = new(
        "bob-id",
        "bob",
        "Bob",
        "Smith",
        "Bob Smith",
        [
            new ResolvedIdentityAddress("bob@example.com", "Bob Smith", "email", false),
            new ResolvedIdentityAddress("1234567890", "Bob Smith", "phone", true)
        ]);

    // Carol hasn't verified anything, but has a registered push device.
    private static readonly ResolvedIdentityProfile Carol = new(
        "carol-id",
        "carol",
        "Carol",
        "Jones",
        "Carol Jones",
        [new ResolvedIdentityAddress("carol@example.com", "Carol Jones", "email", false)]);

    [TestMethod]
    public async Task EachBuyerGetsOneMessageOnTheirFirstReachableChannel()
    {
        var alice = await DispatchOrderCreatedAsync(Alice);
        var bob = await DispatchOrderCreatedAsync(Bob);
        var carol = await DispatchOrderCreatedAsync(Carol);

        // Same intent, same dispatch from Ordering. Identity resolution decides the channel.
        Assert.AreEqual(Providers.Email, alice.Single.ChannelProviderId);
        Assert.AreEqual(Providers.Sms, bob.Single.ChannelProviderId);
        Assert.AreEqual(Providers.Push, carol.Single.ChannelProviderId);
    }

    [TestMethod]
    public async Task EmailIsComposedFromOrderingIdentityAndCatalog()
    {
        var email = (await DispatchOrderCreatedAsync(Alice)).Communication<IEmail>();
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
    public async Task SmsIsShortAndGoesToTheVerifiedPhoneNumber()
    {
        var sms = (await DispatchOrderCreatedAsync(Bob)).Communication<ISms>();
        TestContext.WriteLine(sms.Message);

        Assert.AreEqual("1234567890", sms.To!.Single().Value);
        Assert.AreEqual(
            "We've received your eShop order #123.\nView your order: https://eshop.test/user/orders",
            sms.Message);
    }

    [TestMethod]
    public async Task PushGoesToTheRegisteredDeviceAndCarriesTheOrder()
    {
        var push = (await DispatchOrderCreatedAsync(Carol)).Communication<IPushNotification>();

        Assert.AreEqual("simulated-device-carol-id", push.Recipient.Single().Value);
        Assert.AreEqual("Thanks for your order", push.Title);
        Assert.AreEqual("Order #123 has been received.", push.Body);
        Assert.AreEqual(OrderCreatedPush.OpenOrderAction, push.Data![OrderCreatedPush.ActionKey]);
        Assert.AreEqual("123", push.Data[OrderCreatedPush.OrderIdKey]);
    }

    [TestMethod]
    public async Task EmailIsStillSentWhenCatalogIsUnavailable()
    {
        var email = (await DispatchOrderCreatedAsync(Alice, catalogAvailable: false)).Communication<IEmail>();

        Assert.AreEqual("Thanks for your order #123", email.Subject);
        StringAssert.Contains(email.TextBody, "2 × Wanderer Black Hiking Boots");
        Assert.IsFalse(email.TextBody!.Contains("Daybird"));
    }

    [TestMethod]
    public async Task SimulatedDeliveriesAreRecordedForEachBuyer()
    {
        await using var db = new CommunicationsContext(new DbContextOptionsBuilder<CommunicationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var recorder = new DeliveryReportRecorder(db, TimeProvider.System);

        foreach (var buyer in new[] { Alice, Bob })
        {
            var dispatch = await DispatchOrderCreatedAsync(buyer);
            await recorder.RecordAsync(dispatch.Single, TestContext.CancellationToken);
        }

        var records = await db.Communications
            .OrderBy(record => record.RecipientId)
            .ToListAsync(TestContext.CancellationToken);

        Assert.HasCount(2, records);
        Assert.AreEqual(("alice-id", "Email", "Thanks for your order #123"), (records[0].RecipientId, records[0].ChannelId, records[0].Summary));
        Assert.AreEqual(("bob-id", "Sms"), (records[1].RecipientId, records[1].ChannelId));
    }

    private async Task<Dispatch> DispatchOrderCreatedAsync(ResolvedIdentityProfile buyer, bool catalogAvailable = true)
    {
        var handler = new EshopServicesHandler(buyer, catalogAvailable);
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
        var reported = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        services.AddTransmitly(tly => Providers.Register(tly)
            .AddDeliveryReportHandler(report =>
            {
                reports.Enqueue(report);
                reported.TrySetResult();
                return Task.CompletedTask;
            })
            .AddPlatformIdentityResolver<IdentityServerCustomerIdentityResolver>(CommunicationIdentityTypes.Buyer)
            .AddPlatformIdentityProfileEnricher<PushRegistrationProfileEnricher>(CommunicationIdentityTypes.Buyer)
            .AddOrderCreatedPipeline(new Uri("https://eshop.test/"), Providers));

        await using var serviceProvider = services.BuildServiceProvider();
        var communicationsClient = serviceProvider.GetRequiredService<ICommunicationsClient>();

        // Mirror the hop between Ordering and Communications: the model travels as JSON
        // and the dispatch endpoint converts it before handing it to Transmitly.
        var model = TransactionModelJson.ToModel(JsonSerializer.SerializeToElement(Order))!;

        var result = await communicationsClient.DispatchAsync(
            CommunicationIntents.OrderCreated,
            [new IdentityReference(CommunicationIdentityTypes.Buyer, buyer.Id)],
            TransactionModel.Create(model),
            [],
            cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(
            result.IsSuccessful,
            string.Join("; ", result.Results.Select(r => $"{r?.ChannelId}/{r?.ChannelProviderId}: {r?.Status} {r?.Exception?.Message}")));

        // First match: one message per buyer.
        Assert.HasCount(1, result.Results);
        await reported.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
        return new Dispatch(result, [.. reports]);
    }

    private sealed record Dispatch(IDispatchCommunicationResult Result, IReadOnlyCollection<DeliveryReport> Reports)
    {
        public DeliveryReport Single => Reports.Single();

        public T Communication<T>() => (T)Single.ChannelCommunication!;
    }

    private sealed class EshopServicesHandler(ResolvedIdentityProfile buyer, bool catalogAvailable) : HttpMessageHandler
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
                    Json<IReadOnlyCollection<ResolvedIdentityProfile>>([buyer]),
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
