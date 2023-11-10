using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.PostgreSql;

public class PostgreSqlLockStrategy : LockStrategyBase
{
    private IDbContextTransaction? _transaction;

    public PostgreSqlLockStrategy(PostgreSqlSourceDatabase context) : base(context)
    {
    }

    public sealed override async Task<bool> TryAdquireAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
        bool? lockResult = Context.Database.SqlQueryRaw<bool?>("SELECT pg_try_advisory_lock(PostgreSql-lock-strategy)").AsEnumerable().First();
        await EnsureSchema(cancellationToken).ConfigureAwait(false);
        return lockResult == true;
    }

    public sealed override async Task FreeAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
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
