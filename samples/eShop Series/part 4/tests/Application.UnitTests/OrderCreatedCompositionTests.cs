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
using Transmitly.Model.Configuration;

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
        StringAssert.Contains(email.TextBody, "2 × Wanderer Black Hiking Boots");
        StringAssert.Contains(email.TextBody, "Daybird");
        // The purchased price comes from the order, not Catalog's current price.
        StringAssert.Contains(email.TextBody, "$109.99 each");
        StringAssert.Contains(email.TextBody, "1 × Summit Pro Harness");
        StringAssert.Contains(email.TextBody, "Order total: $309.97");
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
        StringAssert.Contains(email.TextBody, "2 × Wanderer Black Hiking Boots");
        Assert.IsFalse(email.TextBody!.Contains("Daybird"));
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
        var webAppUrl = new Uri("https://eshop.test/");

        // The same OrderCreated registration as the Communications service's Program.cs.
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
            .AddContentModelEnricher<CatalogContentModelEnricher>(options =>
            {
                options.Scope = ContentModelEnricherScope.PerRecipient;
                options.Predicate = context => context.PipelineIntent == CommunicationIntents.OrderCreated;
            })
            .AddPipeline(CommunicationIntents.OrderCreated, pipeline =>
            {
                pipeline.UseAnyMatchPipelineDeliveryStrategy();

                pipeline.AddEmail(
                    "orders@eshop.local".AsIdentityAddress("eShop"),
                    email =>
                    {
                        email.AddChannelProviderFilter(EshopChannelProviders.Email);
                        email.Subject.AddTemplateResolver(context => OrderCreatedEmail.Subject(context));
                        email.TextBody.AddTemplateResolver(context => OrderCreatedEmail.TextBody(context, webAppUrl));
                    });

                pipeline.AddSms(
                    "+15555550100".AsIdentityAddress("eShop"),
                    sms =>
                    {
                        sms.AddChannelProviderFilter(EshopChannelProviders.Sms);
                        sms.Message.AddTemplateResolver(context => OrderCreatedSms.Message(context, webAppUrl));
                    });

                pipeline.AddPushNotification(push =>
                {
                    push.AddChannelProviderFilter(EshopChannelProviders.Push);
                    push.Title.AddTemplateResolver(context => OrderCreatedPush.Title(context));
                    push.Body.AddTemplateResolver(context => OrderCreatedPush.Body(context));
                    push.AddData(OrderCreatedPush.ActionKey, OrderCreatedPush.OpenOrderAction);
                    push.AddDataIfNotNull(OrderCreatedPush.OrderIdKey, context => OrderCreatedPush.OrderId(context));
                });
            }));

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
