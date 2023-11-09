namespace Upgradier.MySql;

public static class MySqlExtensions
{
    public static UpdateBuilder AddMySqlServerEngine(this UpdateBuilder builder)
    {
        builder.AddDatabaseEngines(() => new MySqlEngine());
        return builder;
    }
}
