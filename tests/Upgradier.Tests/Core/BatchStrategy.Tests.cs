using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class BatchStrategy_Tests
{
    private sealed class BatchStrategyMock : BatchStrategyBase
    {
        public BatchStrategyMock(string? environment, string provider, string name) : base(environment, provider, name)
        {
        }
        public string DerivedEnvironment => Environment;
        public override ValueTask<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public override ValueTask<StreamReader> GetBatchContentsAsync(Batch batch, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public void Name_Cannot_Be_null()
    {
        Assert.Throws<ArgumentNullException>("name", () =>
        {
            BatchStrategyMock strategy = new(null, "SqlServer",null);
        });
        Assert.Throws<ArgumentException>("name", () =>
        {
            BatchStrategyMock strategy = new(null, "SqlServer", string.Empty);
        });
    }

    [Fact]
    public void Provider_Cannot_Be_Null_or_Empty()
    {
        Assert.Throws<ArgumentNullException>("provider", () =>
        {
            BatchStrategyMock strategy = new(null, null, "Custom");
        });
        Assert.Throws<ArgumentException>("provider", () =>
        {
            BatchStrategyMock strategy = new(null, string.Empty, "Custom");
        });
    }

    [Fact]
    public void Environment_can_be_null()
    {
        BatchStrategyMock strategy = new(null, "Oracle", "Test");
        Assert.Null(strategy.DerivedEnvironment);
    }
}