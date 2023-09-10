namespace Upgradier.Core;

public interface IDatabaseEngine
{
    string Name { get; }
    ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase);
    SourceDatabase CreateSourceDatabase(string connectionString);
}