using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class WebBatchStrategyTests : IClassFixture<BatchServerFixture>
{
    private readonly BatchServerFixture _batchServerfixture;

    public WebBatchStrategyTests(BatchServerFixture batchServerfixture)
    {
        _batchServerfixture = batchServerfixture;
    }

    [Fact]
    public async Task GetBatches_From_Default_Environment_Returns_2_Files()
    {
        Assert.NotNull(_batchServerfixture.BatchesUri);
        using CancellationTokenSource cancellationTokenSource = new();
        WebBatchStrategy strategy = new(_batchServerfixture.BatchesUri, null, new LogAdapter(null));
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token);
            Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Theory]
    [InlineData(1, "SqlServer")]
    [InlineData(2, "SqlServer")]
    public async Task GetBatchContentsAsync_Get_Contents_By_VersionId(int versionId, string provider)
    {
        Assert.NotNull(_batchServerfixture.BatchesUri);
        string expectedBatchContents = await File.ReadAllTextAsync(Path.Combine("Core", "Batches", provider, $"{versionId}.sql"));
        WebBatchStrategy strategy = new(_batchServerfixture.BatchesUri, null, new LogAdapter(null));
        string actualContents = await strategy.GetBatchContentsAsync(new Batch { VersionId = versionId }, provider, CancellationToken.None);
        Assert.NotNull(actualContents);
        Assert.Equal(expectedBatchContents, actualContents);
    }
}
