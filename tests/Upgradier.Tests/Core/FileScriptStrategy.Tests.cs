using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class FileScriptStrategy_Tests
{
    [Fact]
    public async Task GetAllScripts_Works_When_Base_Directory_Not_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Scripts", "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }

    [Fact]
    public async Task GetAllScripts_Works_When_Base_Directory_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Scripts{Path.DirectorySeparatorChar}", "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }

    [Fact]
    public async Task GetAllScripts_Returns_2_Scripts_In_Null_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Scripts", "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }

    [Fact]
    public async Task GetAllScripts_Returns_3_Scripts_In_Dev_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Scripts", "SqlServer", "Dev");
        IEnumerable<Script> scripts = await strategy.GetScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 3));
        Assert.Equal(3, scripts.Count());
    }

    [Fact]
    public void Ctor_Throws_If_BaseDirectory_Is_Null_Or_Empty()
    {
        Assert.Throws<ArgumentNullException>("baseDirectory", () =>
        {
            new FileScriptStrategy(null, "SqlServer", "Dev");
        });
        Assert.Throws<ArgumentException>("baseDirectory", () =>
        {
            new FileScriptStrategy(string.Empty, "SqlServer", "Dev");
        });
    }

    [Fact]
    public async Task Ctor_Throws_If_File_Index_Json_Not_Exists()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
        {
            FileScriptStrategy strategy = new($"NonExistentDirectory{Path.DirectorySeparatorChar}Scripts", "SqlServer", "Dev");
            IEnumerable<Script> scripts = await strategy.GetScriptsAsync(CancellationToken.None).ConfigureAwait(false);
        });
    }

    [Theory]
    [InlineData(1, "SqlServer", null)]
    [InlineData(2, "SqlServer", null)]
    [InlineData(1, "SqlServer", "Dev")]
    [InlineData(2, "SqlServer", "Dev")]
    [InlineData(3, "SqlServer", "Dev")]
    public async Task GetScriptContentsAsync_Get_Contents_By_VersionId(int versionId, string provider, string? environment)
    {
        string expectedScriptContents = await File.ReadAllTextAsync(Path.Combine("Core", "Scripts", provider, environment ?? string.Empty, $"{versionId}.sql")).ConfigureAwait(false);
        FileScriptStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Scripts", provider, environment);
        using StreamReader actualStreamContent = await strategy.GetScriptContentsAsync(new Script { VersionId = versionId }, CancellationToken.None).ConfigureAwait(false);
        Assert.NotNull(actualStreamContent);
        string actualScriptContents = await actualStreamContent.ReadToEndAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(expectedScriptContents, actualScriptContents);
    }
}
