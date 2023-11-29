namespace Upgradier.Core;

public abstract class BatchStrategyBase : IBatchStrategy
{
    protected BatchStrategyBase(string name, LogAdapter logger, string? environment)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(logger);
        Name = name;
        Logger = logger;
        Environment = environment;
    }

    public string Name { get; }

    public string? Environment { get; }

    protected LogAdapter Logger { get; }

    public abstract Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    public abstract Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken);
}
