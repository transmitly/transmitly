using eShop.ServiceDefaults.Identity;

namespace eShop.Identity.API.Apis;

public static class IdentityApi
{
    public static IEndpointRouteBuilder MapIdentityApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/identities/resolve", ResolveIdentityProfilesAsync)
            .RequireAuthorization(IdentityServerConstants.LocalApi.PolicyName)
            .WithName("ResolveIdentityProfiles");

        return endpoints;
    }

    private static async Task<IResult> ResolveIdentityProfilesAsync(
        ResolveIdentityProfilesRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var identityIds = request.IdentityIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (identityIds.Length == 0 || identityIds.Length != request.IdentityIds.Count)
        {
            return TypedResults.BadRequest();
        }

        var profiles = new List<ResolvedIdentityProfile>(identityIds.Length);

        // UserManager's EF store is scoped and must not be queried concurrently.
        foreach (var identityId in identityIds)
        {
            var user = await userManager.FindByIdAsync(identityId);
            if (user is not null)
            {
                profiles.Add(ToResolvedIdentityProfile(user));
            }
        }

        return TypedResults.Ok<IReadOnlyCollection<ResolvedIdentityProfile>>(profiles);
    }

    private static ResolvedIdentityProfile ToResolvedIdentityProfile(ApplicationUser user)
    {
        var displayName = string.Join(
            ' ',
            new[] { user.Name, user.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var addresses = new List<ResolvedIdentityAddress>();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            addresses.Add(new ResolvedIdentityAddress(
                user.Email,
                displayName,
                "email",
                user.EmailConfirmed));
        }

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            addresses.Add(new ResolvedIdentityAddress(
                user.PhoneNumber,
                displayName,
                "phone",
                user.PhoneNumberConfirmed));
        }

        return new ResolvedIdentityProfile(
            user.Id,
            user.UserName,
            user.Name,
            user.LastName,
            displayName,
            addresses);
    }
}
