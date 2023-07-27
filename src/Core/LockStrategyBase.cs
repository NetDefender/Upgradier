namespace Upgradier.Core;

public abstract class LockStrategyBase : ILockStrategy
{
    protected LockStrategyBase(string? environment, SourceDatabase context)
    {
        Environment = environment;
        Context = context;
    }
    public string? Environment { get; }
    protected SourceDatabase Context { get; }
    public bool IsLockAdquired { get; protected set; }
    public abstract Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);
    public abstract Task FreeAsync(CancellationToken cancellationToken = default);
    public abstract Task EnsureSchema(SourceDatabase sourceDatabase, CancellationToken cancellationToken = default);
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
    Task EnsureSchema(SourceDatabase sourceDatabase, CancellationToken cancellationToken = default);
    bool IsLockAdquired { get; }
    string? Environment { get; }
}