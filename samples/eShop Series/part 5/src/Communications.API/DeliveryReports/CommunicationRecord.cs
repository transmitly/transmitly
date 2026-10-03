namespace eShop.Communications.API.DeliveryReports;

/// <summary>
/// One communication sent to one recipient on one channel, with its latest known status.
/// </summary>
public sealed class CommunicationRecord
{
    public Guid Id { get; set; }

    public required string PipelineIntent { get; set; }

    public string? ChannelId { get; set; }

    public string? ChannelProviderId { get; set; }

    /// <summary>
    /// The provider's id for the message. Later delivery reports from the provider carry it.
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// The platform identity the communication was sent to, when known.
    /// </summary>
    public string? RecipientId { get; set; }

    /// <summary>
    /// A short, human-readable description: the email subject, the SMS text, or the push title.
    /// </summary>
    public string? Summary { get; set; }

    public required string Status { get; set; }

    public bool IsFailure { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<DeliveryEventRecord> Events { get; set; } = [];
}

/// <summary>
/// A single delivery report: what happened, according to which provider, and when.
/// </summary>
public sealed class DeliveryEventRecord
{
    public Guid Id { get; set; }

    public Guid CommunicationRecordId { get; set; }

    public required string EventName { get; set; }

    public string? ChannelProviderId { get; set; }

    public required string Status { get; set; }

    public int StatusCode { get; set; }

    public string? Detail { get; set; }

    /// <summary>
    /// Provider-specific details as JSON, for the cases where the provider-agnostic status isn't enough.
    /// </summary>
    public string? ProviderDetails { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
}
