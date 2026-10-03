#nullable enable

using eShop.Communications.API.DeliveryReports;
using eShop.ServiceDefaults.Communications;
using Microsoft.EntityFrameworkCore;
using Transmitly;
using Transmitly.ChannelProvider.Twilio.Sdk.Sms;
using Transmitly.Delivery;

namespace eShop.Application.UnitTests;

[TestClass]
public class DeliveryReportRecordingTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task TwilioStatusCallbackUpdatesTheDispatchedSms()
    {
        // Registering Twilio is what lets report.Twilio() read Twilio's details,
        // just as it is in the Communications service when Twilio is configured.
        new CommunicationsClientBuilder().AddTwilioSupport(twilio =>
        {
            twilio.AccountSid = "ACtest";
            twilio.AuthToken = "test";
        });

        await using var db = CreateDb();
        var recorder = new DeliveryReportRecorder(db, TimeProvider.System);

        // Twilio accepted the message at dispatch and returned its message id.
        await recorder.RecordAsync(DispatchedSms("SM123"), TestContext.CancellationToken);

        // Later, Twilio posts a status update to the delivery report webhook. Transmitly's
        // Twilio adaptor turns the form post into a provider-agnostic delivery report.
        var callback = new FormRequest(new Dictionary<string, string>
        {
            ["SmsSid"] = "SM123",
            ["MessageSid"] = "SM123",
            ["SmsStatus"] = "undelivered",
            ["MessageStatus"] = "undelivered",
            ["ErrorCode"] = "30003",
            ["To"] = "+15555550123",
            ["From"] = "+15555550100",
            // Added to the status callback URL by Transmitly at dispatch.
            [DeliveryUtil.ChannelIdKey] = Id.Channel.Sms(),
            [DeliveryUtil.ChannelProviderIdKey] = Id.ChannelProvider.Twilio(),
            [DeliveryUtil.PipelineIntentKey] = CommunicationIntents.OrderCreated
        });
        var reports = await new TwilioSmsDeliveryStatusReportAdaptor().AdaptAsync(callback);
        Assert.IsNotNull(reports);

        foreach (var report in reports)
        {
            await recorder.RecordAsync(report, TestContext.CancellationToken);
        }

        var record = await db.Communications.Include(x => x.Events).SingleAsync(TestContext.CancellationToken);
        Assert.AreEqual("buyer-123", record.RecipientId);
        Assert.AreEqual("We've received your eShop order #123.", record.Summary);
        Assert.AreEqual("Undelivered", record.Status);
        Assert.IsTrue(record.IsFailure);
        Assert.HasCount(2, record.Events);

        // The status is provider agnostic; Twilio's own details are still available.
        var statusChanged = record.Events.Single(x => x.EventName == DeliveryReport.Event.StatusChanged());
        StringAssert.Contains(statusChanged.ProviderDetails, "\"ErrorCode\":\"30003\"");
    }

    [TestMethod]
    public async Task ReportsWithoutAResourceIdAreRecordedSeparately()
    {
        await using var db = CreateDb();
        var recorder = new DeliveryReportRecorder(db, TimeProvider.System);

        await recorder.RecordAsync(DispatchedSms(resourceId: null), TestContext.CancellationToken);
        await recorder.RecordAsync(DispatchedSms(resourceId: null), TestContext.CancellationToken);

        Assert.AreEqual(2, await db.Communications.CountAsync(TestContext.CancellationToken));
    }

    private static CommunicationsContext CreateDb() =>
        new(new DbContextOptionsBuilder<CommunicationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static DeliveryReport DispatchedSms(string? resourceId)
    {
        var sms = Substitute.For<ISms>();
        sms.Message.Returns("We've received your eShop order #123.");

        // Transmitly's content model carries the recipient profile under "pid".
        var contentModel = Substitute.For<IContentModel>();
        contentModel.Model.Returns(new Dictionary<string, object?>
        {
            ["pid"] = new Dictionary<string, object?> { ["Id"] = "buyer-123" }
        });

        return new DeliveryReport(
            DeliveryReport.Event.Dispatched(),
            Id.Channel.Sms(),
            Id.ChannelProvider.Twilio(),
            CommunicationIntents.OrderCreated,
            null,
            resourceId,
            CommunicationsStatus.Success("Twilio", "Queued"),
            sms,
            contentModel,
            null);
    }

    private sealed class FormRequest(IReadOnlyDictionary<string, string> form) : IRequestAdaptorContext
    {
        public string? GetQueryValue(string key) => null;

        public string? GetFormValue(string key) => form.GetValueOrDefault(key);

        public string? GetHeaderValue(string key) => null;

        [Obsolete("Use GetQueryValue, GetFormValue or GetHeaderValue instead")]
        public string? GetValue(string key) => GetFormValue(key);

        public string? Content => null;

        public string? PipelineIntent => form.GetValueOrDefault(DeliveryUtil.PipelineIntentKey);

        public string? PipelineId => null;

        public string? ResourceId => form.GetValueOrDefault(DeliveryUtil.ResourceIdKey);
    }
}
