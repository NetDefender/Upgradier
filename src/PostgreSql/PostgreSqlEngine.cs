﻿using Microsoft.EntityFrameworkCore;

namespace Upgradier.PostgreSql;

public class PostgreSqlEngine : IDatabaseEngine
{
    public const string NAME = "PostgreSql";

    public string Name => NAME;

    public virtual ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is PostgreSqlSourceDatabase sqlSourceDatabase)
        {
            return new PostgreSqlLockStrategy(sqlSourceDatabase);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<PostgreSqlSourceDatabase> builder = new DbContextOptionsBuilder<PostgreSqlSourceDatabase>()
            .UseNpgsql(connectionString);
        return new PostgreSqlSourceDatabase(builder.Options);
    }
}