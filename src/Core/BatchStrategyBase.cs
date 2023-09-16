namespace Upgradier.Core;

public abstract class BatchStrategyBase : IBatchStrategy
{
    protected BatchStrategyBase(string? environment, string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name);
        Name = name;
        Environment = environment;
    }

    public string Name { get; }

    public string? Environment { get; }

    public abstract Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    public abstract Task<StreamReader> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken);
}
