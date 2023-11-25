using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.Core;

internal sealed class UpdateEventDispatcher
{
    private readonly IUpdateEvents? _events;
    private readonly LogAdapter _logAdapter;

    public UpdateEventDispatcher(IUpdateEvents? events, LogAdapter logAdapter)
    {
        _events = events;
        _logAdapter = logAdapter;
    }
    public async Task NotifyBeforeBatchProcessingAsync(long version, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if(_events?.BeforeBatchProcessingAsync is not null)
        {
            _logAdapter.LogBeforeBatchProcessingAsyncDispatchEvent(version);
            await _events.BeforeBatchProcessingAsync(version, transaction, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task NotifyAfterBatchProcessedAsync(long version, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if (_events?.AfterBatchProcessedAsync is not null)
        {
            _logAdapter.LogAfterBatchProcessedAsyncDispatchEvent(version);
            await _events.AfterBatchProcessedAsync(version, transaction, cancellationToken).ConfigureAwait(false);
        }
    }
}
