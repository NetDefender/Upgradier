namespace Upgradier.Core;

public interface ILockManager : IDisposable
{
    Task FreeAsync(CancellationToken cancellationToken = default);

    Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);

    Task EnsureSchema(CancellationToken cancellationToken = default);

    string? Environment { get; }
}