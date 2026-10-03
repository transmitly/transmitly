using Transmitly;

namespace eShop.Communications.API;

/// <summary>
/// The provider behind each channel. Email and push always run on Transmitly's simulator.
/// SMS uses Twilio when Twilio credentials are configured, and the simulator otherwise.
/// </summary>
public sealed class EshopChannelProviders
{
    private const string DefaultSmsFromNumber = "+15555550100";

    private readonly TwilioSettings? _twilio;

    private EshopChannelProviders(TwilioSettings? twilio, string? deliveryReportUrl)
    {
        _twilio = twilio;
        DeliveryReportUrl = deliveryReportUrl;
        Sms = twilio is null ? Id.ChannelProvider.Simulation("Sms") : Id.ChannelProvider.Twilio();
        SmsFromNumber = twilio?.FromNumber ?? DefaultSmsFromNumber;
    }

    public string Email { get; } = Id.ChannelProvider.Simulation("Email");
    public string Sms { get; }
    public string Push { get; } = Id.ChannelProvider.Simulation("Push");

    public string SmsFromNumber { get; }

    /// <summary>
    /// The public address of the delivery report webhook, which providers like Twilio call
    /// back with status updates. Without it, providers can't report anything after dispatch.
    /// </summary>
    public string? DeliveryReportUrl { get; }

    public static EshopChannelProviders FromConfiguration(IConfiguration configuration)
    {
        var twilio = configuration.GetSection("Twilio").Get<TwilioSettings>();
        var useTwilio = !string.IsNullOrWhiteSpace(twilio?.AccountSid) && !string.IsNullOrWhiteSpace(twilio.AuthToken);

        return new EshopChannelProviders(
            useTwilio ? twilio : null,
            configuration["Communications:DeliveryReportUrl"] is { Length: > 0 } url ? url : null);
    }

    public static EshopChannelProviders Simulated() => new(twilio: null, deliveryReportUrl: null);

    public CommunicationsClientBuilder Register(CommunicationsClientBuilder builder)
    {
        builder
            .AddSimulationSupport(providerId: "Email")
            .AddSimulationSupport(providerId: "Push");

        if (_twilio is null)
        {
            builder.AddSimulationSupport(providerId: "Sms");
        }
        else
        {
            builder.AddTwilioSupport(twilio =>
            {
                twilio.AccountSid = _twilio.AccountSid;
                twilio.AuthToken = _twilio.AuthToken;
            });
        }

        return builder;
    }

    private sealed record TwilioSettings(string? AccountSid, string? AuthToken, string? FromNumber);
}
