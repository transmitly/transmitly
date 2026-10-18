#nullable enable

using Transmitly;
using Transmitly.Channel.Configuration;
using Transmitly.Delivery;

namespace eShop.Ordering.API.Infrastructure.Communications;

/// <summary>
/// Defers dispatches made inside an open <see cref="PendingCommunications"/> scope until
/// <see cref="TransactionBehavior{TRequest, TResponse}"/> commits. Application code keeps
/// calling <see cref="ICommunicationsClient"/> directly.
/// </summary>
public sealed class TransactionalCommunicationsMiddleware : ICommunicationClientMiddleware
{
    public ICommunicationsClient? CreateClient(
        ICreateCommunicationsClientContext context,
        ICommunicationsClient? previous)
    {
        return previous is null ? null : new DeferringCommunicationsClient(previous);
    }

    private sealed class DeferringCommunicationsClient(ICommunicationsClient inner) : ICommunicationsClient
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
            return DeferOrSend(
                pipelineIntent,
                pipelineId,
                token => inner.DispatchAsync(pipelineIntent, platformIdentities, transactionalModel, dispatchChannelPreferences, pipelineId, cultureInfo, token),
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
            return DeferOrSend(
                pipelineIntent,
                pipelineId,
                token => inner.DispatchAsync(pipelineIntent, identityReferences, transactionalModel, dispatchChannelPreferences, pipelineId, cultureInfo, token),
                cancellationToken);
        }

        public Task DispatchAsync(DeliveryReport report) => inner.DispatchAsync(report);

        public Task DispatchAsync(IReadOnlyCollection<DeliveryReport> reports) => inner.DispatchAsync(reports);

        private static Task<IDispatchCommunicationResult> DeferOrSend(
            string intent,
            string? pipelineId,
            Func<CancellationToken, Task<IDispatchCommunicationResult>> send,
            CancellationToken cancellationToken)
        {
            var pending = PendingCommunications.Current;
            if (pending is null)
            {
                return send(cancellationToken);
            }

            pending.Add(intent, send);

            return Task.FromResult<IDispatchCommunicationResult>(
                new DispatchCommunicationResult([new DeferredDispatchResult(intent, pipelineId)]));
        }
    }

    private sealed record DeferredDispatchResult(string? PipelineIntent, string? PipelineId) : IDispatchResult
    {
        public string? ResourceId => null;
        public CommunicationsStatus Status { get; } =
            CommunicationsStatus.Success("eShop.Ordering", "Deferred until the transaction commits");
        public string? ChannelProviderId => null;
        public string? ChannelId => null;
        public Exception? Exception => null;
    }
}
