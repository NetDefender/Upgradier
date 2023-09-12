namespace Upgradier.Core;

public interface IBatchStrategy
{
    string Name { get; }

    Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    Task<StreamReader> GetBatchContentsAsync(Batch batch, CancellationToken cancellationToken);
}