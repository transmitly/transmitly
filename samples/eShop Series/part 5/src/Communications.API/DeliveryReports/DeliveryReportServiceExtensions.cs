using eShop.Communications.API.DeliveryReports;

namespace Microsoft.Extensions.DependencyInjection;

public static class DeliveryReportServiceExtensions
{
    /// <summary>
    /// Registers the queue Transmitly's delivery report handler writes to, and the background
    /// service that records queued reports in the Communications database.
    /// </summary>
    public static IServiceCollection AddDeliveryReportRecording(
        this IServiceCollection services,
        out DeliveryReportQueue queue)
    {
        queue = new DeliveryReportQueue();
        services.AddSingleton(queue);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<DeliveryReportRecorder>();
        services.AddHostedService<DeliveryReportProcessor>();
        return services;
    }
}
