namespace Upgradier.Core;

public interface IBatchStrategy
{
    string Name { get; }

    Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken);

    Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken);
}