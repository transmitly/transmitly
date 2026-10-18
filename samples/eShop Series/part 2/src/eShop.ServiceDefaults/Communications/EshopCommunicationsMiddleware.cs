using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Transmitly;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;

namespace eShop.ServiceDefaults.Communications;

/// <summary>
/// Forwards dispatches to the Communications service instead of processing pipelines locally.
/// </summary>
public sealed class EshopCommunicationsMiddleware(
    HttpClient httpClient,
    IHostEnvironment environment) : ICommunicationClientMiddleware
{
    internal const string ServiceBaseAddress = "http://communications-api";
    internal const string DispatchEndpoint = ServiceBaseAddress + "/api/communications/dispatch";

    public ICommunicationsClient CreateClient(
        ICreateCommunicationsClientContext context,
        ICommunicationsClient? previous)
    {
        return new ForwardingCommunicationsClient(httpClient, environment.ApplicationName, previous);
    }

    private sealed class ForwardingCommunicationsClient(
        HttpClient httpClient,
        string source,
        ICommunicationsClient? previous) : ICommunicationsClient
    {
        public Task<IDispatchCommunicationResult> DispatchAsync(
            string pipelineIntent,
            IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities,
            ITransactionModel transactionalModel,
            IReadOnlyCollection<string> dispatchChannelPreferences,
            string? pipelineId = null,
            string? cultureInfo = null,
            CancellationToken cancellationToken = default)
        {
            return ForwardAsync(
                pipelineIntent,
                [.. platformIdentities.Select(CommunicationRecipient.From)],
                transactionalModel,
                dispatchChannelPreferences,
                pipelineId,
                cultureInfo,
                cancellationToken);
        }

        public Task<IDispatchCommunicationResult> DispatchAsync(
            string pipelineIntent,
            IReadOnlyCollection<IPlatformIdentityReference> identityReferences,
            ITransactionModel transactionalModel,
            IReadOnlyCollection<string> dispatchChannelPreferences,
            string? pipelineId = null,
            string? cultureInfo = null,
            CancellationToken cancellationToken = default)
        {
            return ForwardAsync(
                pipelineIntent,
                [.. identityReferences.Select(CommunicationRecipient.From)],
                transactionalModel,
                dispatchChannelPreferences,
                pipelineId,
                cultureInfo,
                cancellationToken);
        }

        public Task DispatchAsync(DeliveryReport report) =>
            previous?.DispatchAsync(report) ?? Task.CompletedTask;

        public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> reports) =>
            previous?.DispatchAsync(reports) ?? Task.CompletedTask;

        private async Task<IDispatchCommunicationResult> ForwardAsync(
            string intent,
            IReadOnlyCollection<CommunicationRecipient> recipients,
            ITransactionModel transactionalModel,
            IReadOnlyCollection<string> allowedChannels,
            string? pipelineId,
            string? culture,
            CancellationToken cancellationToken)
        {
            var metadata = CreateMetadata();
            var request = new CommunicationDispatchRequest(
                intent,
                recipients,
                JsonSerializer.SerializeToElement(transactionalModel.Model),
                allowedChannels,
                pipelineId,
                culture,
                metadata);

            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    DispatchEndpoint,
                    request,
                    cancellationToken);

                var remoteResult = await response.Content.ReadFromJsonAsync<CommunicationDispatchResponse>(
                    cancellationToken: cancellationToken);
                var succeeded = response.IsSuccessStatusCode && remoteResult?.IsSuccessful == true;
                var status = succeeded
                    ? CommunicationsStatus.Success("eShop", "Forwarded", detail: metadata.CorrelationId)
                    : CommunicationsStatus.ServerError(
                        "eShop",
                        "Communications service rejected the dispatch",
                        detail: $"HTTP {(int)response.StatusCode}; correlation {metadata.CorrelationId}");

                return new DispatchCommunicationResult(
                    [new ForwardingDispatchResult(metadata.CorrelationId, status, intent, pipelineId)]);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                var status = CommunicationsStatus.ServerError(
                    "eShop",
                    "Communications service unavailable",
                    detail: exception.Message);

                return new DispatchCommunicationResult(
                    [new ForwardingDispatchResult(metadata.CorrelationId, status, intent, pipelineId, exception)]);
            }
        }

        private CommunicationDispatchMetadata CreateMetadata()
        {
            var activity = Activity.Current;

            // Reusing the trace id lets the dispatch be followed across both services' logs.
            var correlationId = activity?.TraceId.ToHexString() ?? Guid.NewGuid().ToString("N");

            var properties = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var (key, value) in activity?.Baggage ?? [])
            {
                // Baggage enumerates from the current activity outward, so the nearest value wins.
                properties.TryAdd(key, value);
            }

            return new CommunicationDispatchMetadata(
                correlationId,
                source,
                DateTimeOffset.UtcNow,
                properties);
        }
    }

    private sealed record ForwardingDispatchResult(
        string? ResourceId,
        CommunicationsStatus Status,
        string? PipelineIntent,
        string? PipelineId,
        Exception? Exception = null) : IDispatchResult
    {
        public string? ChannelProviderId => "eShop.Communications";
        public string? ChannelId => "HTTP";
    }
}
