using Transmitly;
using Transmitly.Delivery;

namespace eShop.Communications.API.DeliveryReports;

/// <summary>
/// Records delivery reports from any provider. The first report for a communication creates
/// its record; later reports with the same provider message id add to its history.
/// </summary>
public sealed class DeliveryReportRecorder(CommunicationsContext db, TimeProvider timeProvider)
{
    public async Task RecordAsync(DeliveryReport report, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var record = await FindAsync(report, cancellationToken);

        if (record is null)
        {
            record = new CommunicationRecord
            {
                Id = Guid.NewGuid(),
                PipelineIntent = report.PipelineIntent ?? "Unknown",
                ChannelId = report.ChannelId,
                ChannelProviderId = report.ChannelProviderId,
                ResourceId = report.ResourceId,
                Status = report.Status.Type,
                CreatedAt = now
            };
            db.Communications.Add(record);
        }

        // A provider's later update carries less context than the dispatch, so only fill gaps.
        record.RecipientId ??= DeliveryReportDetails.RecipientId(report);
        record.Summary ??= DeliveryReportDetails.Summary(report.ChannelCommunication);

        // The latest report wins. Providers don't always report in order, which a
        // production system may want to account for.
        record.Status = report.Status.Type;
        record.IsFailure = report.Status.IsFailure();
        record.UpdatedAt = now;

        // The key is left for EF to generate. A preset key on an event added to an existing
        // communication would make EF treat it as a row to update rather than insert.
        record.Events.Add(new DeliveryEventRecord
        {
            EventName = report.EventName,
            ChannelProviderId = report.ChannelProviderId,
            Status = report.Status.Type,
            StatusCode = report.Status.Code,
            Detail = report.Status.Detail ?? report.Exception?.Message,
            ProviderDetails = DeliveryReportDetails.ProviderDetails(report),
            ReceivedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private Task<CommunicationRecord?> FindAsync(DeliveryReport report, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(report.ResourceId))
        {
            return Task.FromResult<CommunicationRecord?>(null);
        }

        return db.Communications.FirstOrDefaultAsync(
            communication => communication.ChannelId == report.ChannelId
                && communication.ResourceId == report.ResourceId,
            cancellationToken);
    }
}
