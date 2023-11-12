using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class BatchStrategy_Tests
{
    private sealed class BatchStrategyMock : BatchStrategyBase
    {
        public BatchStrategyMock(string name) : base(name)
        {
        }
        public string DerivedEnvironment => Environment;
        public override Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public override Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public void Name_Cannot_Be_null_Or_Empty()
    {
        Assert.Throws<ArgumentNullException>("name", () =>
        {
            BatchStrategyMock strategy = new(null);
        });
        Assert.Throws<ArgumentException>("name", () =>
        {
            BatchStrategyMock strategy = new(string.Empty);
        });
    }
}