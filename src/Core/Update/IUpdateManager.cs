namespace Upgradier.Core;

public interface IUpdateManager
{
    Task<IEnumerable<UpdateResult>> UpdateAsync(CancellationToken cancellationToken = default);
    Task<UpdateResult> UpdateSourceAsync(string sourceName, CancellationToken cancellationToken = default);
}