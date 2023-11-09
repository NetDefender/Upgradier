using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.MySql;

public class MySqlLockStrategy : LockStrategyBase
{
    private IDbContextTransaction? _transaction;

    public MySqlLockStrategy(MySqlSourceDatabase context) : base(context)
    {
    }

    public sealed override async Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
        MySqlLockResult? lockResult = Context.Database.SqlQueryRaw<MySqlLockResult?>("SELECT GET_LOCK('mysql-lock-strategy', 0);").AsEnumerable().First();
        await EnsureSchema(cancellationToken).ConfigureAwait(false);
        return lockResult == MySqlLockResult.Granted;
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
