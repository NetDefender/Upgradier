namespace Upgradier.Core;

public interface IUpdateManager : IDisposable
{
    bool IsUpdating();
    ValueTask<IEnumerable<UpdateResult>> Update(CancellationToken cancellationToken = default);
    ValueTask<UpdateResult> UpdateSource(string sourceName, CancellationToken cancellationToken = default);
}