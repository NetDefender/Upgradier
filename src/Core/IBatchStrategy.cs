namespace Upgradier.Core;

public interface IBatchStrategy
{
    string Name { get; }

    ValueTask<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    ValueTask<StreamReader> GetBatchContentsAsync(Batch batch, CancellationToken cancellationToken);
}