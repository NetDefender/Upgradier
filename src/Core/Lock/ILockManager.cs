using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.Core;

public interface ILockManager : IDisposable
{
    Task FreeAsync(CancellationToken cancellationToken = default);

    Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);

    IDbContextTransaction? Transaction { get; }

    string? Environment { get; }
}