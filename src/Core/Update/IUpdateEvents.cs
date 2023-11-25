using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.Core;

public interface IUpdateEvents
{
    Func<long, IDbContextTransaction, CancellationToken, Task> BeforeBatchProcessingAsync { get; }

    Func<long, IDbContextTransaction, CancellationToken, Task> AfterBatchProcessedAsync { get; }
}
