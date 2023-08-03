using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class WebScriptStrategyTests : IClassFixture<ScriptServerFixture>
{
    private readonly ScriptServerFixture _scriptServerfixture;

    public WebScriptStrategyTests(ScriptServerFixture scriptServerfixture)
    {
        _scriptServerfixture = scriptServerfixture;
    }

    [Fact]
    public async Task GetScripts_From_Default_Environment_Returns_2_Files()
    {
        Assert.NotNull(_scriptServerfixture.ScriptsUri);
        using CancellationTokenSource cancellationTokenSource = new();
        WebScriptStrategy strategy = new(_scriptServerfixture.ScriptsUri, "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }
}
