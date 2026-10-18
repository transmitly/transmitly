using System.Text.Json;
using Transmitly;

namespace eShop.ServiceDefaults.Communications;

/// <summary>
/// The transport representation of the context Transmitly needs to continue a dispatch.
/// Pipeline definitions intentionally do not live in this shared contract.
/// </summary>
public sealed record CommunicationDispatchRequest(
    string Intent,
    IReadOnlyCollection<CommunicationRecipient> Recipients,
    JsonElement Model,
    IReadOnlyCollection<string> AllowedChannels,
    string? PipelineId,
    string? Culture,
    CommunicationDispatchMetadata Metadata);

/// <summary>
/// A recipient either carries the addresses to use, or only an identity reference
/// (<see cref="Id"/> and <see cref="Type"/>) that Communications resolves.
/// </summary>
public sealed record CommunicationRecipient(
    string? Id,
    string? Type,
    IReadOnlyCollection<CommunicationAddress> Addresses)
{
    internal static CommunicationRecipient From(IPlatformIdentityProfile identity) =>
        new(identity.Id, identity.Type, [.. identity.Addresses.Select(CommunicationAddress.From)]);

    internal static CommunicationRecipient From(IPlatformIdentityReference identity) =>
        new(identity.Id, identity.Type, []);
}

public sealed record CommunicationAddress(
    string Value,
    string? Display,
    string? Type)
{
    internal static CommunicationAddress From(IPlatformIdentityAddress address) =>
        new(address.Value, address.Display, address.Type);
}

/// <summary>
/// Describes the dispatch itself rather than the communication.
/// </summary>
/// <param name="CorrelationId">Ties the originating service's logs to the Communications service's logs.</param>
/// <param name="Source">The service that dispatched the intent.</param>
/// <param name="DispatchedAt">When the originating service forwarded the dispatch.</param>
/// <param name="Properties">Ambient context from the dispatching operation, taken from the current activity's baggage.</param>
public sealed record CommunicationDispatchMetadata(
    string CorrelationId,
    string Source,
    DateTimeOffset DispatchedAt,
    IReadOnlyDictionary<string, string?> Properties);

public sealed record CommunicationDispatchResponse(
    bool IsSuccessful,
    int ResultCount,
    string CorrelationId);
