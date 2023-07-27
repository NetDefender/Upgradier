namespace Upgradier.Core;

public class Source
{
    public Source(string name, string provider, string connectionString)
    {
        Name = name;
        Provider = provider;
        ConnectionString = connectionString;
    }
    public string Name { get; }
    public string Provider { get; }
    public string ConnectionString { get; }
}