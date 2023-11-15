using Microsoft.EntityFrameworkCore.Storage;
using Upgradier.Core;

namespace Ugradier.Core;

public interface IUpdateEvents
{
    Func<long, IDbContextTransaction, CancellationToken, Task> BeforeExecuteBatchAsync { get; }

    Func<long, IDbContextTransaction, CancellationToken, Task> AfterExecuteBatchAsync { get; }
}
