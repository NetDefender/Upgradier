namespace Upgradier.Core;

public interface IProviderFactory
{
    string Name { get; }
    ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase);
    IScriptAdapter CreateScriptAdapter();
    SourceDatabase CreateSourceDatabase(string connectionString);
}