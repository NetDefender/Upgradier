using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class FileBatchStrategy_Tests
{
    [Fact]
    public async Task GetAllBatches_Works_When_Base_Directory_Not_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", null);
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Fact]
    public async Task GetAllBatches_Works_When_Base_Directory_Ends_With_DirectorySeparator()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches{Path.DirectorySeparatorChar}", null);
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Fact]
    public async Task GetAllBatches_Returns_2_Batches_In_Null_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", null);
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Fact]
    public async Task GetAllBatches_Returns_3_Batches_In_Dev_Environment()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches", "Dev");
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 3));
        Assert.Equal(3, batches.Count());
    }

    [Fact]
    public void Ctor_Throws_If_BaseDirectory_Is_Null_Or_Empty()
    {
        Assert.Throws<ArgumentNullException>("baseDirectory", () =>
        {
            new FileBatchStrategy(null, "Dev");
        });
        Assert.Throws<ArgumentException>("baseDirectory", () =>
        {
            new FileBatchStrategy(string.Empty, "Dev");
        });
    }

    [Fact]
    public async Task Ctor_Throws_If_File_Index_Json_Not_Exists()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
        {
            FileBatchStrategy strategy = new($"NonExistentDirectory{Path.DirectorySeparatorChar}Batches", "Dev");
            IEnumerable<Batch> batches = await strategy.GetBatchesAsync(CancellationToken.None).ConfigureAwait(false);
        });
    }

    [Theory]
    [InlineData(1, "SqlServer", null)]
    [InlineData(2, "SqlServer", null)]
    [InlineData(1, "SqlServer", "Dev")]
    [InlineData(2, "SqlServer", "Dev")]
    [InlineData(3, "SqlServer", "Dev")]
    public async Task GetBatchContentsAsync_Get_Contents_By_VersionId(int versionId, string provider, string? environment)
    {
        string expectedBatchContents = await File.ReadAllTextAsync(Path.Combine("Core", "Batches", provider, environment ?? string.Empty, $"{versionId}.sql")).ConfigureAwait(false);
        FileBatchStrategy strategy = new($"Core{Path.DirectorySeparatorChar}Batches",  environment);
        using StreamReader actualStreamContent = await strategy.GetBatchContentsAsync(new Batch { VersionId = versionId }, provider, CancellationToken.None).ConfigureAwait(false);
        Assert.NotNull(actualStreamContent);
        string actualBatchContents = await actualStreamContent.ReadToEndAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(expectedBatchContents, actualBatchContents);
    }
}
