using System.Text.Json;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class WebSourceProvider_Tests : IClassFixture<SourceServerFixture>
{
    private readonly Uri _sourceServerUri;

    public WebSourceProvider_Tests(SourceServerFixture sourceServerFixture)
    {
        Assert.NotNull(sourceServerFixture.SourcesUri);
        _sourceServerUri = sourceServerFixture.SourcesUri;
    }

    [Fact]
    public void Throws_When_BaseUri_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new WebSourceProvider("ProviderName", null, "Sources.json", null, new LogAdapter(null), null));
    }

    [Fact]
    public void Throws_When_BaseUri_Is_Not_AbsoluteUri()
    {
        UriBuilder builder = new(_sourceServerUri)
        {
            Path = "Example-Path"
        };
        Uri relativeUri = _sourceServerUri.MakeRelativeUri(builder.Uri);
        Assert.Throws<DirectoryNotFoundException>(() => new WebSourceProvider("ProviderName", relativeUri, "Sources.json", null, new LogAdapter(null), null));
    }

    [Fact]
    public async Task ConfigureRequestMessage_Throws_When_Null_Is_Passed_Explicitly()
    {
        WebSourceProvider strategy = new("ProviderName", _sourceServerUri, "Sources.json", null, new LogAdapter(null), null);
        Assert.Throws<ArgumentNullException>(() => strategy.ConfigureRequestMessage(null));
    }

    [Theory]
    [InlineData(null)] // Returns 2 sources
    [InlineData("Dev")] // Returns 1 source
    public async Task GetSourcesAsync_Returns_Expected_Content(string environment)
    {
        using CancellationTokenSource cancellationTokenSource = new();
        WebSourceProvider provider = new("ProviderName", _sourceServerUri, "Sources.json", null, new LogAdapter(null), environment);
        IEnumerable<Source> actualSources = await provider.GetSourcesAsync(CancellationToken.None);

        string fileName = environment.IsNullOrEmptyOrWhiteSpace() ? "Sources.json" : $"Sources.{environment}.json";
        byte[] content = await File.ReadAllBytesAsync(Path.Combine("Core/Files", fileName));
        IEnumerable<Source> expectedSources = JsonSerializer.Deserialize<IEnumerable<Source>>(content);

        Assert.Equal(expectedSources.Count(), actualSources.Count());
        using IEnumerator<Source> expectedSourcesEnumerator = expectedSources.GetEnumerator();
        using IEnumerator<Source> sourcesEnumerator = actualSources.GetEnumerator();

        while (expectedSourcesEnumerator.MoveNext())
        {
            sourcesEnumerator.MoveNext();

            Source expectedSource = expectedSourcesEnumerator.Current;
            Source actualSource = sourcesEnumerator.Current;

            Assert.Equal(expectedSource.Name, actualSource.Name);
            Assert.Equal(expectedSource.Provider, actualSource.Provider);
            Assert.Equal(expectedSource.ConnectionString, actualSource.ConnectionString);
        }
    }
}