﻿using Microsoft.EntityFrameworkCore;

namespace Upgradier.PostgreSql;

public class PostgreSqlEngine : IDatabaseEngine
{
    public const string NAME = "PostgreSql";

    private LogAdapter _logger;
    private readonly string? _environment;

    public PostgreSqlEngine(LogAdapter logger, string? environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public string Name => NAME;

    public virtual ILockManager CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is PostgreSqlSourceDatabase sqlSourceDatabase)
        {
            return new PostgreSqlLockManager(sqlSourceDatabase, _logger, _environment);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<PostgreSqlSourceDatabase> builder = new DbContextOptionsBuilder<PostgreSqlSourceDatabase>()
            .UseNpgsql(connectionString);
        return new PostgreSqlSourceDatabase(builder.Options, _logger, _environment);
    }
}
