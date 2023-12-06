using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.DatabaseEngines.SqlServer;

public class SqlLockManager : LockManagerBase
{
    private IDbContextTransaction? _transaction;

    public SqlLockManager(SqlSourceDatabase context, LogAdapter logger, string? environment) 
        : base(context, logger, environment)
    {
    }

    public sealed override async Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
        SqlLockResult lockResult = Context.Database.SqlQueryRaw<SqlLockResult>("""
            DECLARE @LockResult INT;
            EXEC @LockResult = sp_getapplock @Resource = N'sqlserver-lock-strategy', @LockMode = 'Exclusive', @LockOwner = 'Transaction';
            SELECT @LockResult;
            """).AsEnumerable().First();
        await Context.EnsureSchema(cancellationToken).ConfigureAwait(false);
        return lockResult is SqlLockResult.Granted or SqlLockResult.Success;
    }

    public sealed override async Task FreeAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override IDbContextTransaction? Transaction { get => _transaction; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
        }
    }
}