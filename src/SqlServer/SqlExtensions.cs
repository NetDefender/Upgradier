namespace Upgradier.SqlServer;

public static class SqlExtensions
{
    public static UpdateBuilder AddSqlProviderFactory(this UpdateBuilder builder, string batchesDirectoryOrBaseUrl, string? environment = null)
    {
        builder.AddProviderFactories(() => new SqlFactory(environment, batchesDirectoryOrBaseUrl));
        return builder;
    }
}