namespace Upgradier.SqlServer;

public static class SqlExtensions
{
    public static UpdateBuilder AddSqlServerEngine(this UpdateBuilder builder, string? environment = null)
    {
        builder.AddDatabaseEngines(() => new SqlEngine(environment));
        return builder;
    }
}