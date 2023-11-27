using Microsoft.EntityFrameworkCore;

namespace Upgradier.Core;

public class SourceDatabase : DbContext
{
    public SourceDatabase(DbContextOptions options, LogAdapter logger) : base(options)
    {
        Environment = EnvironmentVariables.GetExecutionEnvironment();
        Logger = logger;
    }

    public virtual DbSet<DatabaseVersion> Version { get; set; }

    public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }

    protected string? Environment { get; }

    protected LogAdapter Logger { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

    public async Task ChangeCurrentVersionAsync(Source source, DatabaseVersion currentVersion, long newVersion, CancellationToken cancellationToken)
    {
        Logger.LogChangingCurrentVersion(source, currentVersion.VersionId, newVersion);
        Remove(currentVersion);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        currentVersion.VersionId = newVersion;
        await AddAsync(currentVersion, cancellationToken).ConfigureAwait(false);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ExecuteBatchAsync(Source source, Batch batch, string batchContents, CancellationToken cancellationToken)
    {
        Logger.LogExecutingBatch(source, batch, batchContents);
        await Database.ExecuteSqlRawAsync(batchContents, cancellationToken).ConfigureAwait(false);
    }
}
