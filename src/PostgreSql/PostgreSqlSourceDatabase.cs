using Microsoft.EntityFrameworkCore;

namespace Upgradier.PostgreSql;

public class PostgreSqlSourceDatabase : SourceDatabase
{
    public PostgreSqlSourceDatabase(DbContextOptions<PostgreSqlSourceDatabase> options, LogAdapter logger) : base(options, logger)
    {
    }
}
