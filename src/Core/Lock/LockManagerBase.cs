using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.Core;

public abstract class LockManagerBase : ILockManager
{
    protected LockManagerBase(SourceDatabase context, LogAdapter logger, string? environment)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        Environment = environment;
        Context = context;
        Logger = logger;
    }

    public string? Environment { get; }

    protected internal SourceDatabase Context { get; }

    protected LogAdapter Logger { get; }

    public virtual IDbContextTransaction? Transaction { get; }

    public abstract Task AdquireAsync(CancellationToken cancellationToken = default);

    public abstract Task FreeAsync(CancellationToken cancellationToken = default);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected abstract void Dispose(bool disposing);
}