using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class BatchStrategy_Tests
{
    private sealed class BatchStrategyMock : BatchStrategyBase
    {
        public BatchStrategyMock(string? environment, string name) : base(environment, name)
        {
        }
        public string DerivedEnvironment => Environment;
        public override Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public override Task<StreamReader> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public void Name_Cannot_Be_null()
    {
        Assert.Throws<ArgumentNullException>("name", () =>
        {
            BatchStrategyMock strategy = new(null, null);
        });
        Assert.Throws<ArgumentException>("name", () =>
        {
            BatchStrategyMock strategy = new(null, string.Empty);
        });
    }


    [Fact]
    public void Environment_can_be_null()
    {
        BatchStrategyMock strategy = new(null, "Test");
        Assert.Null(strategy.DerivedEnvironment);
    }
}