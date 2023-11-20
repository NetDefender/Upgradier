namespace Upgradier.Core;

public interface IDatabaseEngine
{
    string Name { get; }

    ILockManager CreateLockStrategy(SourceDatabase sourceDatabase);

    SourceDatabase CreateSourceDatabase(string connectionString);
}