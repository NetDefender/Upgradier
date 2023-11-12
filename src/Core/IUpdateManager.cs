namespace Upgradier.Core;

public interface IUpdateManager
{
    Task<IEnumerable<UpdateResult>> Update(CancellationToken cancellationToken = default);
    Task<UpdateResult> UpdateSource(string sourceName, CancellationToken cancellationToken = default);
}