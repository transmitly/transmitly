namespace eShop.Communications.API.PushRegistrations;

/// <summary>
/// The push device tokens registered for a platform identity. Device tokens are
/// communications data: Identity doesn't own them, and neither does Ordering.
/// </summary>
public interface IPushRegistrationStore
{
    Task<IReadOnlyCollection<string>> GetDeviceTokensAsync(string identityId, CancellationToken cancellationToken = default);
}
