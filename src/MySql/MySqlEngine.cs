using Microsoft.EntityFrameworkCore;

namespace Upgradier.MySql;

public class MySqlEngine : IDatabaseEngine
{
    public const string NAME = "MySql";

    public string Name => NAME;

    public virtual ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase)
    {
        if (sourceDatabase is MySqlSourceDatabase sqlSourceDatabase)
        {
            return new MySqlLockStrategy(sqlSourceDatabase);
        }
        throw new InvalidCastException(nameof(sourceDatabase));
    }

    public virtual SourceDatabase CreateSourceDatabase(string connectionString)
    {
        DbContextOptionsBuilder<MySqlSourceDatabase> builder = new DbContextOptionsBuilder<MySqlSourceDatabase>()
            .UseMySQL(connectionString);
        return new MySqlSourceDatabase(builder.Options);
    }
}
