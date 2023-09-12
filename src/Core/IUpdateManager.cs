namespace Upgradier.Core;

public interface IUpdateManager : IDisposable
{
    bool IsUpdating();
    Task<IEnumerable<UpdateResult>> Update(CancellationToken cancellationToken = default);
    Task<UpdateResult> UpdateSource(string sourceName, CancellationToken cancellationToken = default);
}