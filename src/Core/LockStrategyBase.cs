using Ugradier.Core;

namespace Upgradier.Core;

public abstract class LockStrategyBase : ILockStrategy
{
    protected LockStrategyBase(SourceDatabase context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Environment = EnvironmentVariables.GetExecutionEnvironment();
        Context = context;
    }
    public string? Environment { get; }
    protected internal SourceDatabase Context { get; }
    public abstract Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);
    public abstract Task FreeAsync(CancellationToken cancellationToken = default);
    public abstract Task EnsureSchema(CancellationToken cancellationToken = default);
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected abstract void Dispose(bool disposing);
}

public interface ILockStrategy : IDisposable
{
    Task FreeAsync(CancellationToken cancellationToken = default);
    Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);
    Task EnsureSchema(CancellationToken cancellationToken = default);
    string? Environment { get; }
}