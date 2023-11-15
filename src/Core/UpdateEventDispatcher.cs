using Microsoft.EntityFrameworkCore.Storage;

namespace Ugradier.Core;

internal sealed class UpdateEventDispatcher
{
    private readonly IUpdateEvents? _events;

    public UpdateEventDispatcher(IUpdateEvents? events)
    {
        _events = events;
    }
    public async Task NotifyBeforeExecuteBatchAsync(long version, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if(_events?.BeforeExecuteBatchAsync is not null)
        {
            await _events.BeforeExecuteBatchAsync(version, transaction, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task NotifyAfterExecuteBatchAsync(long version, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if (_events?.AfterExecuteBatchAsync is not null)
        {
            await _events.AfterExecuteBatchAsync(version, transaction, cancellationToken).ConfigureAwait(false);
        }
    }
}
