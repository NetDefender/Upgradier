namespace Upgradier.SqlServer;

public static class SqlExtensions
{
    public static UpdateBuilder AddSqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(new SqlEngine());
        return builder;
    }
}