using System.Text.Json;
using Transmitly;

namespace eShop.ServiceDefaults.Communications;

/// <summary>
/// The transport representation of the context Transmitly needs to continue a dispatch.
/// Pipeline definitions intentionally do not live in this shared contract.
/// </summary>
public sealed record CommunicationDispatchRequest(
    string Intent,
    IReadOnlyCollection<CommunicationIdentityReference> Recipients,
    JsonElement Model,
    IReadOnlyCollection<string> AllowedChannels,
    string? PipelineId,
    string? Culture,
    string CorrelationId);

public sealed record CommunicationIdentityReference(
    string? Id,
    string? Type)
{
    internal static CommunicationIdentityReference From(IPlatformIdentityProfile identity) =>
        new(identity.Id, identity.Type);

    internal static CommunicationIdentityReference From(IPlatformIdentityReference identity) =>
        new(identity.Id, identity.Type);
}

public sealed record CommunicationDispatchResponse(
    bool IsSuccessful,
    int ResultCount,
    string CorrelationId);
