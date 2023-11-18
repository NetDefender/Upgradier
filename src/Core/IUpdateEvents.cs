using Microsoft.EntityFrameworkCore.Storage;
using Upgradier.Core;

namespace Ugradier.Core;

public interface IUpdateEvents
{
    Func<long, IDbContextTransaction, CancellationToken, Task> BeforeBatchProcessingAsync { get; }

    Func<long, IDbContextTransaction, CancellationToken, Task> AfterBatchProcessedAsync { get; }
}
