using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.SqlServer;

public class SqlLockStrategy : LockStrategyBase
{
    private IDbContextTransaction? _transaction;

    public SqlLockStrategy(SqlSourceDatabase context) : base(context)
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
        await EnsureSchema(cancellationToken).ConfigureAwait(false);
        return lockResult is SqlLockResult.Granted or SqlLockResult.Success;
    }

    public sealed override async Task FreeAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
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