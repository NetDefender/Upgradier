using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Upgradier.Core;

public class SourceDatabase : DbContext
{
    public SourceDatabase(DbContextOptions options, LogAdapter logger, string? environment) : base(options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Environment = environment;
        Logger = logger;
    }

    public virtual DbSet<DatabaseVersion> Version { get; set; }

    public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }

    protected string? Environment { get; }

    protected LogAdapter Logger { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        Logger.LogSourceModelCreating();
        modelBuilder.Entity<DatabaseVersion>(entity =>
        {
            entity.ToTable("__Version");
            entity.HasKey(version => version.VersionId);
            entity.Property(version => version.VersionId)
                .IsRequired()
                .ValueGeneratedNever();
        });
        modelBuilder.Entity<MigrationHistory>(entity =>
        {
            entity.ToTable("__UpgradientMigrationHistory");
            entity.HasKey(migration => migration.MigrationId);
            entity.Property(migration => migration.MigrationId)
                .IsRequired()
                .ValueGeneratedNever();
        });
    }

    public async Task ChangeCurrentVersionAsync(DatabaseVersion currentVersion, long newVersion, CancellationToken cancellationToken)
    {
        Logger.LogChangingCurrentVersion(currentVersion.VersionId, newVersion);
        Remove(currentVersion);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        currentVersion.VersionId = newVersion;
        await AddAsync(currentVersion, cancellationToken).ConfigureAwait(false);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        Logger.LogChangedCurrentVersion(newVersion);
    }

    public async Task ExecuteBatchAsync(string batchContents, CancellationToken cancellationToken)
    {
        Logger.LogExecuteBatch(batchContents);
        await Database.ExecuteSqlRawAsync(batchContents, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task EnsureSchema(CancellationToken cancellationToken = default)
    {
        Logger.LogEnsuringSchema();
        Assembly resourceAssembly = GetType().Assembly;
        Dictionary<int, string> migrationBatches = resourceAssembly.GetManifestResourceNames().Where(resource => resource.EndsWith(".sql"))
            .ToDictionary(resource => resource.ResourceId());
        Stream? startupResource = resourceAssembly.GetManifestResourceStream(migrationBatches[0]);
        ArgumentNullException.ThrowIfNull(startupResource);
        using StreamReader startupBatch = new(startupResource, leaveOpen: false);
        await Database.ExecuteSqlRawAsync(await startupBatch.ReadToEndAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        MigrationHistory? currentMigration = await MigrationHistory.FirstOrDefaultAsync(cancellationToken);
        int currentMigrationValue = currentMigration?.MigrationId ?? 0;

        foreach (int migrationNeeded in migrationBatches.Keys.Where(batchKey => batchKey > currentMigrationValue).Order())
        {
            Logger.LogExecutingMigration(migrationNeeded);
            Stream? migrationResource = resourceAssembly.GetManifestResourceStream(migrationBatches[migrationNeeded]);
            ArgumentNullException.ThrowIfNull(migrationResource);
            using StreamReader migrationBatch = new(migrationResource, leaveOpen: false);
            await Database.ExecuteSqlRawAsync(await migrationBatch.ReadToEndAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        }
    }
}
