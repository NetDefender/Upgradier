﻿using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.DatabaseEngines.MySql;

public class MySqlLockManager : LockManagerBase
{
    private IDbContextTransaction? _transaction;

    public MySqlLockManager(MySqlSourceDatabase context, LogAdapter logger, string? environment) 
        : base(context, logger, environment)
    {
    }

    public override IDbContextTransaction? Transaction { get => _transaction; }

    public sealed override async Task AdquireAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
        MySqlLockResult? lockResult = Context.Database.SqlQueryRaw<MySqlLockResult?>("SELECT GET_LOCK('upgradier-lock', 0);").AsEnumerable().First();
        await Context.EnsureSchema(cancellationToken).ConfigureAwait(false);
        LockNotAdquiredException.ThrowIfNotAdquired(lockResult == MySqlLockResult.Granted);
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
