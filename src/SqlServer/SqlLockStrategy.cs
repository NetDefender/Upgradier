using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.SqlServer;

public class SqlLockStrategy : LockStrategyBase
{
    private IDbContextTransaction? _transaction;
    public SqlLockStrategy(string? environment, SqlSourceDatabase context) : base(environment, context)
    {
    }
    public sealed override async Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken).ConfigureAwait(false);
        SqlLockResult lockResult = Context.Database.SqlQueryRaw<SqlLockResult>("""
            DECLARE @LockResult INT;
            EXEC @LockResult = sp_getapplock @Resource = N'sqlserver-lock-strategy', @LockMode = 'Exclusive', @LockOwner = 'Transaction';
            SELECT @LockResult;
            """).AsEnumerable().First();
        await EnsureSchema(cancellationToken).ConfigureAwait(false);
        return lockResult == SqlLockResult.Granted;
    }
    public sealed override async Task FreeAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task EnsureSchema(CancellationToken cancellationToken = default)
    {
        Assembly resourceAssembly = Context.GetType().Assembly;
        Dictionary<int, string> migrationScripts = resourceAssembly.GetManifestResourceNames().Where(resource => resource.EndsWith(".sql"))
            .ToDictionary(resource => resource.ResourceId());
        Stream? startupResource = resourceAssembly.GetManifestResourceStream(migrationScripts[0]);
        ArgumentNullException.ThrowIfNull(startupResource);
        using StreamReader startupScript = new(startupResource, leaveOpen: false);
        await Context.Database.ExecuteSqlRawAsync(await startupScript.ReadToEndAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        MigrationHistory? currentMigration = await Context.MigrationHistory.FirstOrDefaultAsync(cancellationToken);
        int currentMigrationValue = currentMigration?.MigrationId ?? 0;

        foreach (int migrationNeeded in migrationScripts.Keys.Where(scriptKey => scriptKey > currentMigrationValue).Order())
        {
            Stream? migrationResource = resourceAssembly.GetManifestResourceStream(migrationScripts[migrationNeeded]);
            ArgumentNullException.ThrowIfNull(migrationResource);
            using StreamReader migrationScript = new(migrationResource, leaveOpen: false);
            await Context.Database.ExecuteSqlRawAsync(await migrationScript.ReadToEndAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
        }
    }
}