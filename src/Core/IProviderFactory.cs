namespace Upgradier.Core;

public interface IProviderFactory
{
    string Name { get; }
    ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase);
    IScriptStrategy CreateScriptStrategy();
    SourceDatabase CreateSourceDatabase(string connectionString);
}