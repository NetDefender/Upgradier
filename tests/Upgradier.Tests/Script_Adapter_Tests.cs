using Upgradier.Core;

namespace Upgradier.Tests;

public class Script_Adapter_Tests
{
    private sealed class ScriptAdapterMock : ScriptAdapterBase
    {
        public ScriptAdapterMock(string? environment, string provider, string name) : base(environment, provider, name)
        {
        }

        public string DerivedEnvironment => Environment;

        public override ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public override ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public void Provider_and_Name_are_not_null()
    {
        ScriptAdapterMock adapter = new (null, "SqlServer", "Test");

        Assert.NotNull(adapter.Provider);
        Assert.NotNull(adapter.Name);
    }

    [Fact]
    public void Environment_can_be_null()
    {
        string environment = null;
        ScriptAdapterMock adapter = new(environment, "Oracle", "Test");
        Assert.Null(adapter.DerivedEnvironment);
    }

    [Fact]
    public void Environment_can_be_not_null()
    {
        string environment = "dev";
        ScriptAdapterMock adapter = new(environment, "SqlServer", "Test");
        Assert.NotNull(adapter.DerivedEnvironment);
    }
}