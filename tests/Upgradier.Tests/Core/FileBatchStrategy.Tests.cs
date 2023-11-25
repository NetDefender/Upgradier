using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class FileBatchStrategy_Tests
{
    [Fact]
    public async Task GetAllBatches_Works_When_Base_Directory_Not_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", new LogAdapter(null));
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Fact]
    public async Task GetAllBatches_Works_When_Base_Directory_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches{Path.DirectorySeparatorChar}", new LogAdapter(null));
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Fact]
    public async Task GetAllBatches_Returns_2_Batches_In_Null_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", new LogAdapter(null));
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Fact]
    public async Task GetAllBatches_Returns_3_Batches_In_Dev_Environment()
    {
        EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", new LogAdapter(null));
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 3));
        Assert.Equal(3, batches.Count());
        EnvironmentVariables.SetExecutionEnvironment(null);
    }

    [Fact]
    public void Ctor_Throws_If_BaseDirectory_Is_Null_Or_Empty()
    {
        Assert.Throws<ArgumentNullException>("baseDirectory", () =>
        {
            new FileBatchStrategy(null, new LogAdapter(null));
        });
        Assert.Throws<ArgumentException>("baseDirectory", () =>
        {
            new FileBatchStrategy(string.Empty, new LogAdapter(null));
        });
    }

    [Fact]
    public async Task Ctor_Throws_If_File_Index_Json_Not_Exists()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
        {
            FileBatchStrategy strategy = new($"NonExistentDirectory{Path.DirectorySeparatorChar}Batches", new LogAdapter(null));
            IEnumerable<Batch> batches = await strategy.GetBatchesAsync(CancellationToken.None);
        });
    }

    [Theory]
    [InlineData(1, "SqlServer")]
    [InlineData(2, "SqlServer")]
    public async Task GetBatchContentsAsync_Get_Contents_By_VersionId(int versionId, string provider)
    {
        string expectedBatchContents = await File.ReadAllTextAsync(Path.Combine("Core", "Batches", provider, $"{versionId}.sql"));
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", new LogAdapter(null));
        string actualContents = await strategy.GetBatchContentsAsync(new Batch { VersionId = versionId }, provider, CancellationToken.None);
        Assert.NotNull(actualContents);
        Assert.Equal(expectedBatchContents, actualContents);
    }
}
