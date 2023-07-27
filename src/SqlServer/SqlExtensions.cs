namespace Upgradier.SqlServer;

public static class SqlExtensions
{
    public static UpdateBuilder AddSqlProviderFactory(this UpdateBuilder builder, string scriptsDirectoryOrBaseUrl, string? environment = null)
    {
        builder.AddProviderFactories(() => new SqlFactory(environment, scriptsDirectoryOrBaseUrl));
        return builder;
    }
}