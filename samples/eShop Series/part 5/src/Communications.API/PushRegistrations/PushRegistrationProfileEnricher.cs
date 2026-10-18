using Transmitly;
using Transmitly.Channel.Push;
using Transmitly.PlatformIdentity.Configuration;

namespace eShop.Communications.API.PushRegistrations;

/// <summary>
/// Adds the buyer's registered push devices to the profile Identity resolved, so the
/// push channel has an address to deliver to.
/// </summary>
public sealed class PushRegistrationProfileEnricher(IPushRegistrationStore registrations)
    : IPlatformIdentityProfileEnricher
{
    public async Task EnrichIdentityProfileAsync(IPlatformIdentityProfile identityProfile)
    {
        if (identityProfile is not CustomerIdentityProfile customer)
        {
            return;
        }

        foreach (var token in await registrations.GetDeviceTokensAsync(customer.Id))
        {
            customer.AddAddress(new PlatformIdentityAddress(
                token,
                type: PlatformIdentityAddress.Types.DeviceToken()));
        }
    }
}
