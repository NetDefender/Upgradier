namespace Upgradier.Core;

public interface IProviderFactory
{
    string Name { get; }
    ILockStrategy CreateLockStrategy(SourceDatabase sourceDatabase);
    IBatchStrategy CreateBatchStrategy();
    SourceDatabase CreateSourceDatabase(string connectionString);
}