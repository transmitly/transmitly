using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using eShop.ServiceDefaults.Communications;
using eShop.ServiceDefaults.Identity;
using Transmitly;
using Transmitly.PlatformIdentity.Configuration;

namespace eShop.Communications.API;

/// <summary>
/// Resolves eShop customer identities through the service that owns them.
/// </summary>
public sealed class IdentityServerCustomerIdentityResolver(HttpClient httpClient)
    : IPlatformIdentityResolver
{
    public async Task<IReadOnlyCollection<IPlatformIdentityProfile>?> ResolveIdentityProfiles(
        IReadOnlyCollection<IPlatformIdentityReference> identityReferences)
    {
        if (identityReferences.Count == 0)
        {
            return [];
        }

        var accessToken = await GetAccessTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/identities/resolve")
        {
            Content = JsonContent.Create(new ResolveIdentityProfilesRequest(
                [.. identityReferences.Select(reference => reference.Id!)]))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var profiles = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ResolvedIdentityProfile>>()
            ?? [];

        return [.. profiles.Select(ToCustomerIdentityProfile)];
    }

    private async Task<string> GetAccessTokenAsync()
    {
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "communications",
                ["client_secret"] = "secret",
                ["scope"] = "IdentityServerApi"
            })
        };

        using var tokenResponse = await httpClient.SendAsync(tokenRequest);
        tokenResponse.EnsureSuccessStatusCode();

        var token = await tokenResponse.Content.ReadFromJsonAsync<ClientCredentialsToken>();
        return token?.AccessToken
            ?? throw new InvalidOperationException("IdentityServer did not return an access token.");
    }

    private static CustomerIdentityProfile ToCustomerIdentityProfile(ResolvedIdentityProfile profile)
    {
        var addresses = profile.Addresses.Select(address =>
            (IPlatformIdentityAddress)new PlatformIdentityAddress(
                address.Value,
                address.Display,
                new Dictionary<string, string?>
                {
                    ["verified"] = address.IsVerified.ToString()
                },
                address.Type));

        return new CustomerIdentityProfile(
            profile.Id,
            profile.UserName,
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            addresses);
    }

    private sealed record ClientCredentialsToken(
        [property: JsonPropertyName("access_token")] string AccessToken);
}

public sealed class CustomerIdentityProfile(
    string id,
    string? userName,
    string? firstName,
    string? lastName,
    string displayName,
    IEnumerable<IPlatformIdentityAddress> addresses) : IPlatformIdentityProfile
{
    private readonly List<IPlatformIdentityAddress> _addresses = [.. addresses];

    public string Id { get; } = id;
    public string? UserName { get; } = userName;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public string DisplayName { get; } = displayName;
    public IReadOnlyCollection<IPlatformIdentityAddress> Addresses => _addresses;

    string? IPlatformIdentityProfile.Type => CommunicationIdentityTypes.Buyer;

    /// <summary>
    /// Lets profile enrichers add addresses that Identity doesn't own, such as push device tokens.
    /// </summary>
    public void AddAddress(IPlatformIdentityAddress address) => _addresses.Add(address);
}
