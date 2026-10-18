namespace eShop.ServiceDefaults.Communications;

/// <summary>
/// Platform identity types shared by eShop services. Communications uses the type
/// to pick the resolver that turns an identity reference into a recipient profile.
/// </summary>
public static class CommunicationIdentityTypes
{
    public const string Buyer = nameof(Buyer);
}
