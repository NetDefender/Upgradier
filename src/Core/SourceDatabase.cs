using Microsoft.EntityFrameworkCore;

namespace Upgradier.Core;

public class SourceDatabase : DbContext
{
    public SourceDatabase(DbContextOptions options) : this(null, options)
    {
    }
    public SourceDatabase(string? environment, DbContextOptions options) : base(options)
    {
        Environment = environment;
    }
    public virtual DbSet<DatabaseVersion> Version { get; set; }
    public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
    protected string? Environment { get; }
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
}
