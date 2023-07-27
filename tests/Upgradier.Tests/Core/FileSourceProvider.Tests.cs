using System.Text.Json;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class FileSourceProvider_Tests
{

    [Fact]
    public void Throws_When_BaseDirectory_Is_Null_Or_Empty()
    {
        Assert.Throws<ArgumentNullException>(() => new FileSourceProvider("ProviderName", null, "Sources.json", new LogAdapter(null), null));
        Assert.Throws<ArgumentException>(() => new FileSourceProvider("ProviderName", string.Empty, "Sources.json", new LogAdapter(null), null));
    }

    [Fact]
    public async Task GetSourcesAsync_Returns_2_Sources_From_Null_Environment()
    {
        FileSourceProvider provider = new ("ProviderName", "Core/Files", "Sources.json", new LogAdapter(null), null);
        IEnumerable<Source> sources = await provider.GetSourcesAsync(CancellationToken.None);
        Assert.Equal(2, sources.Count());
    }

    [Fact]
    public async Task GetSourcesAsync_Returns_1_Source_From_Dev_Environment()
    {
        FileSourceProvider provider = new("ProviderName", "Core/Files", "Sources.json", new LogAdapter(null), "Dev");
        IEnumerable<Source> sources = await provider.GetSourcesAsync(CancellationToken.None);
        Assert.Equal(1, sources.Count());
    }


    [Theory]
    [InlineData(null)]
    [InlineData("Dev")]
    public async Task GetSourcesAsync_Returns_Expected_Content(string environment)
    {
        FileSourceProvider provider = new("ProviderName", "Core/Files", "Sources.json", new LogAdapter(null), environment);
        IEnumerable<Source> actualSources = await provider.GetSourcesAsync(CancellationToken.None);

        string fileName = environment.IsNullOrEmptyOrWhiteSpace() ? "Sources.json" : $"Sources.{environment}.json";
        byte[] content = await File.ReadAllBytesAsync(Path.Combine("Core/Files", fileName));
        IEnumerable<Source> expectedSources = JsonSerializer.Deserialize<IEnumerable<Source>>(content);
        Assert.Equal(expectedSources.Count(), actualSources.Count());

        using IEnumerator<Source> expectedSourcesEnumerator = expectedSources.GetEnumerator();
        using IEnumerator<Source> sourcesEnumerator = actualSources.GetEnumerator();

        while(expectedSourcesEnumerator.MoveNext())
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
