using Microsoft.Extensions.Logging;

namespace KeywordFighting.Configuration;
internal interface ICancellationTokenSource
{
    CancellationToken Token { get; }
    void Cancel();
    void Reset();
}

internal class RenewableCancellationTokenSource(ILogger<RenewableCancellationTokenSource> logger) : ICancellationTokenSource
{
    private CancellationTokenSource _source = new();

    public CancellationToken Token => _source.Token;

    public void Cancel()
    {
        logger.LogDebug("Cancelling cancellation token source...");
        _source.Cancel();
        logger.LogDebug("Cancellation token source cancelled.");
    }

    public void Reset()
    {
        logger.LogDebug("Resetting cancellation token source...");
        var oldSource = _source;
        _source = new CancellationTokenSource();
        oldSource.Dispose();
        logger.LogDebug("Cancellation token source reset.");
    }
}