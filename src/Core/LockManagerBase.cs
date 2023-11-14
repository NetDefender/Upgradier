using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Upgradier.Core;

public abstract class LockManagerBase : ILockManager
{
    protected LockManagerBase(SourceDatabase context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Environment = EnvironmentVariables.GetExecutionEnvironment();
        Context = context;
    }

    public string? Environment { get; }

    protected internal SourceDatabase Context { get; }

    public abstract Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);

    public abstract Task FreeAsync(CancellationToken cancellationToken = default);

    public virtual async Task EnsureSchema(CancellationToken cancellationToken = default)
    {
        Assembly resourceAssembly = GetType().Assembly;
        Dictionary<int, string> migrationBatches = resourceAssembly.GetManifestResourceNames().Where(resource => resource.EndsWith(".sql"))
            .ToDictionary(resource => resource.ResourceId());
        Stream? startupResource = resourceAssembly.GetManifestResourceStream(migrationBatches[0]);
        ArgumentNullException.ThrowIfNull(startupResource);
        using StreamReader startupBatch = new(startupResource, leaveOpen: false);
        await Context.Database.ExecuteSqlRawAsync(await startupBatch.ReadToEndAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        MigrationHistory? currentMigration = await Context.MigrationHistory.FirstOrDefaultAsync(cancellationToken);
        int currentMigrationValue = currentMigration?.MigrationId ?? 0;

        foreach (int migrationNeeded in migrationBatches.Keys.Where(batchKey => batchKey > currentMigrationValue).Order())
        {
            Stream? migrationResource = resourceAssembly.GetManifestResourceStream(migrationBatches[migrationNeeded]);
            ArgumentNullException.ThrowIfNull(migrationResource);
            using StreamReader migrationBatch = new(migrationResource, leaveOpen: false);
            await Context.Database.ExecuteSqlRawAsync(await migrationBatch.ReadToEndAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected abstract void Dispose(bool disposing);
}

public interface ILockManager : IDisposable
{
    Task FreeAsync(CancellationToken cancellationToken = default);

    Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default);

    Task EnsureSchema(CancellationToken cancellationToken = default);

    string? Environment { get; }
}