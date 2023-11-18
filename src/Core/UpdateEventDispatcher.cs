using Microsoft.EntityFrameworkCore.Storage;

namespace Ugradier.Core;

internal sealed class UpdateEventDispatcher
{
    private readonly IUpdateEvents? _events;

    public UpdateEventDispatcher(IUpdateEvents? events)
    {
        _events = events;
    }
    public async Task NotifyBeforeBatchProcessingAsync(long version, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if(_events?.BeforeBatchProcessingAsync is not null)
        {
            await _events.BeforeBatchProcessingAsync(version, transaction, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task NotifyAfterBatchProcessedAsync(long version, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if (_events?.AfterBatchProcessedAsync is not null)
        {
            await _events.AfterBatchProcessedAsync(version, transaction, cancellationToken).ConfigureAwait(false);
        }
    }
}
