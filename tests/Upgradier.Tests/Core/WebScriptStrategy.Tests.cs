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

    [Theory]
    [InlineData(1, "SqlServer", null)]
    [InlineData(2, "SqlServer", null)]
    [InlineData(1, "SqlServer", "Dev")]
    [InlineData(2, "SqlServer", "Dev")]
    [InlineData(3, "SqlServer", "Dev")]
    public async Task GetScriptContentsAsync_Get_Contents_By_VersionId(int versionId, string provider, string? environment)
    {
        Assert.NotNull(_scriptServerfixture.ScriptsUri);
        string expectedScriptContents = await File.ReadAllTextAsync(Path.Combine("Core", "Scripts", provider, environment ?? string.Empty, $"{versionId}.sql")).ConfigureAwait(false);
        WebScriptStrategy strategy = new(_scriptServerfixture.ScriptsUri, provider, environment);
        using StreamReader actualStreamContent = await strategy.GetScriptContentsAsync(new Script { VersionId = versionId }, CancellationToken.None).ConfigureAwait(false);
        Assert.NotNull(actualStreamContent);
        string actualScriptContents = await actualStreamContent.ReadToEndAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(expectedScriptContents, actualScriptContents);
    }
}
