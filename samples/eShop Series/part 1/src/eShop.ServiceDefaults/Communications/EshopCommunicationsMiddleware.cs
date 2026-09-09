using System.Net.Http.Json;
using System.Text.Json;
using Transmitly;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;

namespace eShop.ServiceDefaults.Communications;

public sealed class EshopCommunicationsMiddleware(HttpClient httpClient) : ICommunicationClientMiddleware
{
    internal const string ServiceBaseAddress = "http://communications-api";
    internal const string DispatchEndpoint = ServiceBaseAddress + "/api/communications/dispatch";

    public ICommunicationsClient CreateClient(
        ICreateCommunicationsClientContext context,
        ICommunicationsClient? previous)
    {
        return new ForwardingCommunicationsClient(httpClient, previous);
    }

    private sealed class ForwardingCommunicationsClient(
        HttpClient httpClient,
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
                [.. platformIdentities.Select(CommunicationIdentityReference.From)],
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
                [.. identityReferences.Select(CommunicationIdentityReference.From)],
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
            IReadOnlyCollection<CommunicationIdentityReference> recipients,
            ITransactionModel transactionalModel,
            IReadOnlyCollection<string> allowedChannels,
            string? pipelineId,
            string? culture,
            CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString("N");
            var request = new CommunicationDispatchRequest(
                intent,
                recipients,
                JsonSerializer.SerializeToElement(transactionalModel.Model),
                allowedChannels,
                pipelineId,
                culture,
                correlationId);

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
                    ? CommunicationsStatus.Success("eShop", "Forwarded", detail: correlationId)
                    : CommunicationsStatus.ServerError(
                        "eShop",
                        "Communications service rejected the dispatch",
                        detail: $"HTTP {(int)response.StatusCode}; correlation {correlationId}");

                return new DispatchCommunicationResult(
                    [new ForwardingDispatchResult(correlationId, status, intent, pipelineId)]);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                var status = CommunicationsStatus.ServerError(
                    "eShop",
                    "Communications service unavailable",
                    detail: exception.Message);

                return new DispatchCommunicationResult(
                    [new ForwardingDispatchResult(correlationId, status, intent, pipelineId, exception)]);
            }
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
