using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.Core;

public abstract class LockManagerBase : ILockManager
{
    protected LockManagerBase(SourceDatabase context, LogAdapter logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        Environment = EnvironmentVariables.GetExecutionEnvironment();
        Context = context;
        Logger = logger;
    }

    public string? Environment { get; }

    protected internal SourceDatabase Context { get; }

    protected LogAdapter Logger { get; }

    public virtual IDbContextTransaction? Transaction { get; }

    public abstract Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);

    public abstract Task FreeAsync(CancellationToken cancellationToken = default);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected abstract void Dispose(bool disposing);
}

