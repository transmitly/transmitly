using System.Text.Json;
using Transmitly;
using Transmitly.Delivery;

namespace eShop.Communications.API.DeliveryReports;

/// <summary>
/// Reads what a delivery report knows about its communication. Reports raised at dispatch
/// carry the rendered communication and content model; later provider updates usually
/// carry only the provider's message id and status.
/// </summary>
internal static class DeliveryReportDetails
{
    private const int MaxSummaryLength = 500;

    // Transmitly keeps the recipient's profile under this key in the content model.
    private const string PlatformIdentityKey = "pid";

    public static string? RecipientId(DeliveryReport report)
    {
        if (report.ContentModel?.Model is IDictionary<string, object?> model
            && model.TryGetValue(PlatformIdentityKey, out var identity)
            && identity is IDictionary<string, object?> profile
            && profile.TryGetValue(nameof(IPlatformIdentityProfile.Id), out var id))
        {
            return id?.ToString();
        }

        return null;
    }

    public static string? Summary(object? communication)
    {
        var summary = communication switch
        {
            IEmail email => email.Subject,
            ISms sms => sms.Message,
            IPushNotification push => push.Title,
            _ => null
        };

        return summary is { Length: > MaxSummaryLength } ? summary[..MaxSummaryLength] : summary;
    }

    /// <summary>
    /// Provider-specific details, for when the provider-agnostic status isn't enough.
    /// </summary>
    public static string? ProviderDetails(DeliveryReport report)
    {
        if (report.ChannelProviderId?.StartsWith(Id.ChannelProvider.Twilio(), StringComparison.OrdinalIgnoreCase) == true
            && report.ChannelId == Id.Channel.Sms())
        {
            var sms = report.Twilio().Sms;
            return JsonSerializer.Serialize(new
            {
                sms.MessageSid,
                MessageStatus = sms.MessageStatus?.ToString(),
                sms.ErrorCode,
                sms.To,
                sms.From
            });
        }

        return null;
    }
}
