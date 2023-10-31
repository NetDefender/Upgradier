using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public sealed class SqlSourceDatabase : SourceDatabase
{
    public SqlSourceDatabase(DbContextOptions<SqlSourceDatabase> options) : base(options)
    {
    }
}
