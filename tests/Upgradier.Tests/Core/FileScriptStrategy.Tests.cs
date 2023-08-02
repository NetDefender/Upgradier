using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class FileScriptStrategy_Tests
{
    [Fact]
    public async Task GetAllScripts_Works_When_Base_Directory_Not_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new("Core/Scripts", "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetAllScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }

    [Fact]
    public async Task GetAllScripts_Works_When_Base_Directory_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new("Core/Scripts/", "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetAllScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }

    [Fact]
    public async Task GetAllScripts_Returns_2_Scripts_In_Null_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new("Core/Scripts", "SqlServer", null);
        IEnumerable<Script> scripts = await strategy.GetAllScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, scripts.Count());
    }

    [Fact]
    public async Task GetAllScripts_Returns_3_Scripts_In_Dev_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileScriptStrategy strategy = new("Core/Scripts", "SqlServer", "Dev");
        IEnumerable<Script> scripts = await strategy.GetAllScriptsAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 2));
        Assert.NotNull(scripts.FirstOrDefault(s => s.VersionId == 3));
        Assert.Equal(3, scripts.Count());
    }

    [Fact]
    public void FileStrategy_Throws_If_BaseDirectory_Is_Null_Or_Empty()
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
    public async Task FileStrategy_Throws_If_File_Index_Json_Not_Exists()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
        {
            FileScriptStrategy strategy = new("NonExistentDirectory/Scripts", "SqlServer", "Dev");
            IEnumerable<Script> scripts = await strategy.GetAllScriptsAsync(CancellationToken.None).ConfigureAwait(false);
        });
    }
}
