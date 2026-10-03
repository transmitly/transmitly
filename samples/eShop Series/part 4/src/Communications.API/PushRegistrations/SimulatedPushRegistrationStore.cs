namespace eShop.Communications.API.PushRegistrations;

/// <summary>
/// eShop's apps don't register for push notifications, so the sample gives every buyer
/// one simulated device. A real store would hold the tokens a mobile app registers.
/// </summary>
public sealed class SimulatedPushRegistrationStore : IPushRegistrationStore
{
    public Task<IReadOnlyCollection<string>> GetDeviceTokensAsync(string identityId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<string>>([$"simulated-device-{identityId}"]);
}
