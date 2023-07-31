using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class Script_Strategy_Tests
{
    private sealed class ScriptStrategyMock : ScriptStrategyBase
    {
        public ScriptStrategyMock(string? environment, string provider, string name) : base(environment, provider, name)
        {
        }

        public string DerivedEnvironment => Environment;

        public override ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public override ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public void Provider_and_Name_are_not_null()
    {
        ScriptStrategyMock strategy = new(null, "SqlServer", "Test");

        Assert.NotNull(strategy.Provider);
        Assert.NotNull(strategy.Name);
    }

    [Fact]
    public void Environment_can_be_null()
    {
        ScriptStrategyMock strategy = new(null, "Oracle", "Test");
        Assert.Null(strategy.DerivedEnvironment);
    }

    [Fact]
    public void Environment_can_be_not_null()
    {
        string environment = "dev";
        ScriptStrategyMock strategy = new(environment, "SqlServer", "Test");
        Assert.NotNull(strategy.DerivedEnvironment);
    }
}