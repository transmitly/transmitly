using Transmitly;

namespace eShop.Communications.API;

/// <summary>
/// The provider behind each channel. The sample runs every channel on Transmitly's simulator;
/// connecting a real provider means changing its registration and its id here, nothing else.
/// </summary>
public static class EshopChannelProviders
{
    public static string Email { get; } = Id.ChannelProvider.Simulation("Email");
    public static string Sms { get; } = Id.ChannelProvider.Simulation("Sms");
    public static string Push { get; } = Id.ChannelProvider.Simulation("Push");

    public static CommunicationsClientBuilder AddEshopChannelProviders(this CommunicationsClientBuilder builder) =>
        builder
            // Stand-ins for an email provider (SMTP, SendGrid), an SMS provider (Twilio), and a push provider (Firebase).
            .AddSimulationSupport(providerId: "Email")
            .AddSimulationSupport(providerId: "Sms")
            .AddSimulationSupport(providerId: "Push");
}
