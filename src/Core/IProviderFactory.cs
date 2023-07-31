namespace Upgradier.Core;

public interface IProviderFactory
{
    string Name { get; }
    ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase);
    IScriptStragegy CreateScriptStrategy();
    SourceDatabase CreateSourceDatabase(string connectionString);
}