#nullable enable

using Transmitly;

namespace eShop.Ordering.API.Infrastructure.Communications;

/// <summary>
/// Holds communication dispatches made while an Ordering transaction is open, so they are
/// sent only after the transaction commits. If the transaction fails, they are dropped.
/// </summary>
/// <remarks>
/// Pending dispatches live in memory. If the process stops between the commit and
/// <see cref="FlushAsync"/>, they are lost.
/// </remarks>
public sealed class PendingCommunications : IDisposable
{
    private static readonly AsyncLocal<PendingCommunications?> s_current = new();

    private readonly PendingCommunications? _outer;
    private readonly List<PendingDispatch> _dispatches = [];

    private PendingCommunications(PendingCommunications? outer) => _outer = outer;

    internal static PendingCommunications? Current => s_current.Value;

    public static PendingCommunications Begin()
    {
        var pending = new PendingCommunications(s_current.Value);
        s_current.Value = pending;
        return pending;
    }

    internal void Add(string intent, Func<CancellationToken, Task<IDispatchCommunicationResult>> send) =>
        _dispatches.Add(new PendingDispatch(intent, send));

    public async Task FlushAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        var dispatches = _dispatches.ToArray();
        _dispatches.Clear();

        foreach (var dispatch in dispatches)
        {
            try
            {
                var result = await dispatch.Send(cancellationToken);
                if (!result.IsSuccessful)
                {
                    logger.LogWarning("The {Intent} communication was not dispatched successfully.", dispatch.Intent);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "The {Intent} communication could not be dispatched.", dispatch.Intent);
            }
        }
    }

    public void Dispose()
    {
        _dispatches.Clear();

        if (s_current.Value == this)
        {
            s_current.Value = _outer;
        }
    }

    private sealed record PendingDispatch(
        string Intent,
        Func<CancellationToken, Task<IDispatchCommunicationResult>> Send);
}
