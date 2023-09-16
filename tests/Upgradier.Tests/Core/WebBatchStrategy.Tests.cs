﻿using Upgradier.Core;

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
        WebBatchStrategy strategy = new(_batchServerfixture.BatchesUri, null);
        IEnumerable<Batch> batches = await strategy.GetBatchesAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 1));
        Assert.NotNull(batches.FirstOrDefault(s => s.VersionId == 2));
        Assert.Equal(2, batches.Count());
    }

    [Theory]
    [InlineData(1, "SqlServer", null)]
    [InlineData(2, "SqlServer", null)]
    [InlineData(1, "SqlServer", "Dev")]
    [InlineData(2, "SqlServer", "Dev")]
    [InlineData(3, "SqlServer", "Dev")]
    public async Task GetBatchContentsAsync_Get_Contents_By_VersionId(int versionId, string provider, string? environment)
    {
        Assert.NotNull(_batchServerfixture.BatchesUri);
        string expectedBatchContents = await File.ReadAllTextAsync(Path.Combine("Core", "Batches", provider, environment ?? string.Empty, $"{versionId}.sql")).ConfigureAwait(false);
        WebBatchStrategy strategy = new(_batchServerfixture.BatchesUri, environment);
        using StreamReader actualStreamContent = await strategy.GetBatchContentsAsync(new Batch { VersionId = versionId }, provider, CancellationToken.None).ConfigureAwait(false);
        Assert.NotNull(actualStreamContent);
        string actualBatchContents = await actualStreamContent.ReadToEndAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(expectedBatchContents, actualBatchContents);
    }
}
