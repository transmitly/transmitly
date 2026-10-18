#nullable enable

using System.Net;
using eShop.Communications.API.DeliveryReports;
using eShop.ServiceDefaults.Communications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Transmitly;
using Transmitly.Delivery;

namespace eShop.Application.UnitTests;

[TestClass]
public class DeliveryReportWebhookTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task TwilioStatusCallbackBecomesADeliveryReport()
    {
        var received = new TaskCompletionSource<DeliveryReport>(TaskCreationOptions.RunContinuationsAsynchronously);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddTransmitly(tly => tly
            .AddTwilioSupport(twilio =>
            {
                twilio.AccountSid = "ACtest";
                twilio.AuthToken = "test";
            })
            .AddDeliveryReportHandler(report =>
            {
                received.TrySetResult(report);
                return Task.CompletedTask;
            }));
        builder.Services
            .AddControllers(options => options.AddTransmitlyDeliveryReportModelBinders())
            .AddApplicationPart(typeof(DeliveryReportsController).Assembly);

        await using var app = builder.Build();
        app.MapControllers();
        await app.StartAsync(TestContext.CancellationToken);

        // The status callback URL Twilio's dispatcher builds from the configured StatusCallbackUrl:
        // the webhook address plus the routing details the Twilio adaptor uses to recognize its
        // own requests. Twilio assigns the message id later, so there's no resource id yet.
        var callbackUrl = new Uri("http://localhost/api/communications/delivery-reports")
            .AddPipelineContext(string.Empty, CommunicationIntents.OrderCreated, null, Id.Channel.Sms(), Id.ChannelProvider.Twilio());

        using var response = await app.GetTestClient().PostAsync(
            callbackUrl,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["SmsSid"] = "SM123",
                ["MessageSid"] = "SM123",
                ["SmsStatus"] = "delivered",
                ["MessageStatus"] = "delivered",
                ["AccountSid"] = "ACtest"
            }),
            TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var report = await received.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
        Assert.AreEqual(DeliveryReport.Event.StatusChanged(), report.EventName);
        Assert.AreEqual(Id.Channel.Sms(), report.ChannelId);
        Assert.AreEqual("SM123", report.ResourceId);
        Assert.AreEqual(CommunicationIntents.OrderCreated, report.PipelineIntent);
        Assert.AreEqual("Delivered", report.Status.Type);
    }
}
