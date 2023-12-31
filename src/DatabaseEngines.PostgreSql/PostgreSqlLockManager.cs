﻿using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.DatabaseEngines.PostgreSql;

public class PostgreSqlLockManager : LockManagerBase
{
    private IDbContextTransaction? _transaction;

    public PostgreSqlLockManager(PostgreSqlSourceDatabase context, LogAdapter logger, string? environment) 
        : base(context, logger, environment)
    {
    }

    public override IDbContextTransaction? Transaction { get => _transaction; }

    public sealed override async Task AdquireAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
        bool? lockAdquired = Context.Database.SqlQueryRaw<bool?>("SELECT pg_try_advisory_lock(20230412)").AsEnumerable().First();
        await Context.EnsureSchema(cancellationToken).ConfigureAwait(false);
        LockNotAdquiredException.ThrowIfNotAdquired(lockAdquired.GetValueOrDefault());
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
