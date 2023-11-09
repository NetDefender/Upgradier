using Microsoft.EntityFrameworkCore;

namespace Upgradier.MySql;

public class MySqlSourceDatabase : SourceDatabase
{
    public MySqlSourceDatabase(DbContextOptions<MySqlSourceDatabase> options) : base(options)
    {
    }
}