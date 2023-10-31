using Microsoft.EntityFrameworkCore;

namespace Upgradier.SqlServer;

public class SqlEngine : IDatabaseEngine
{
    public const string NAME = "SqlServer";

    public SqlEngine()
    {
    }

    public string Name => NAME;

    public virtual ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is SqlSourceDatabase sqlSourceDatabase)
        {
            return new SqlLockStrategy(sqlSourceDatabase);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<SqlSourceDatabase> builder = new DbContextOptionsBuilder<SqlSourceDatabase>()
            .UseSqlServer(connectionString);
        return new SqlSourceDatabase(builder.Options);
    }
}