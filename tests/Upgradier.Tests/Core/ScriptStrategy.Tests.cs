using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class ScriptStrategy_Tests
{
    private sealed class ScriptStrategyMock : ScriptStrategyBase
    {
        public ScriptStrategyMock(string? environment, string provider, string name) : base(environment, provider, name)
        {
        }
        public string DerivedEnvironment => Environment;
        public override ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public override ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public void Name_Cannot_Be_null()
    {
        Assert.Throws<ArgumentNullException>("name", () =>
        {
            ScriptStrategyMock strategy = new(null, "SqlServer",null);
        });
        Assert.Throws<ArgumentException>("name", () =>
        {
            ScriptStrategyMock strategy = new(null, "SqlServer", string.Empty);
        });
    }

    [Fact]
    public void Provider_Cannot_Be_Null_or_Empty()
    {
        Assert.Throws<ArgumentNullException>("provider", () =>
        {
            ScriptStrategyMock strategy = new(null, null, "Custom");
        });
        Assert.Throws<ArgumentException>("provider", () =>
        {
            ScriptStrategyMock strategy = new(null, string.Empty, "Custom");
        });
    }

    [Fact]
    public void Environment_can_be_null()
    {
        ScriptStrategyMock strategy = new(null, "Oracle", "Test");
        Assert.Null(strategy.DerivedEnvironment);
    }
}