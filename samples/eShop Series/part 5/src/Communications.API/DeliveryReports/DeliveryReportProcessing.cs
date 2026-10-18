using System.Threading.Channels;
using Transmitly.Delivery;

namespace eShop.Communications.API.DeliveryReports;

/// <summary>
/// Hands delivery reports from Transmitly to the background recorder. Transmitly raises
/// reports from inside a dispatch or a webhook request, so recording happens elsewhere.
/// </summary>
public sealed class DeliveryReportQueue
{
    private readonly Channel<DeliveryReport> _reports = Channel.CreateUnbounded<DeliveryReport>(
        new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(DeliveryReport report) => _reports.Writer.TryWrite(report);

    public IAsyncEnumerable<DeliveryReport> ReadAllAsync(CancellationToken cancellationToken) =>
        _reports.Reader.ReadAllAsync(cancellationToken);
}

/// <summary>
/// Records queued delivery reports one at a time, so a quick provider update can't race
/// the report it follows.
/// </summary>
public sealed class DeliveryReportProcessor(
    DeliveryReportQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<DeliveryReportProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var report in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var recorder = scope.ServiceProvider.GetRequiredService<DeliveryReportRecorder>();
                await recorder.RecordAsync(report, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(
                    ex,
                    "Could not record the {EventName} report for {ChannelId} resource {ResourceId}.",
                    report.EventName,
                    report.ChannelId,
                    report.ResourceId);
            }
        }
    }
}
