namespace eShop.ServiceDefaults.Identity;

public sealed record ResolveIdentityProfilesRequest(
    IReadOnlyCollection<string> IdentityIds);

public sealed record ResolvedIdentityProfile(
    string Id,
    string? UserName,
    string? FirstName,
    string? LastName,
    string DisplayName,
    IReadOnlyCollection<ResolvedIdentityAddress> Addresses);

public sealed record ResolvedIdentityAddress(
    string Value,
    string? Display,
    string Type,
    bool IsVerified);
