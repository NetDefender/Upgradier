using Microsoft.EntityFrameworkCore;

namespace Upgradier.PostgreSql;

public class PostgreSqlSourceDatabase : SourceDatabase
{
    public PostgreSqlSourceDatabase(DbContextOptions<PostgreSqlSourceDatabase> options, LogAdapter logger, string? environment) 
        : base(options, logger, environment)
    {
    }
}
