using System.Text.Json;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class EncrypteddFileSourceProvider_Tests
{

    private readonly byte[] SymmetricKey = [0xfa, 0x27, 0xc0, 0x47, 0xf7, 0xe6, 0xd8, 0x33, 0x52, 0x70, 0x35, 0x63, 0x08, 0x3c, 0xc2, 0xdd, 0x25, 0x7c, 0x63, 0xdd, 0x39, 0x1d, 0xe0, 0xba, 0xe2, 0x2c, 0xf8, 0x06, 0x16, 0x84, 0x48, 0xe8];
    private readonly byte[] SymmetricIv = [0x40, 0xba, 0x57, 0xe1, 0x63, 0x11, 0x75, 0x1c, 0xe0, 0x7d, 0x88, 0x3f, 0x6e, 0x53, 0xf5, 0x1b];

    [Fact]
    public void Throws_When_Encryptor_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new EncryptedFileSourceProvider("ProviderName", "Core/Files", "Sources.json", new LogAdapter(null), null, null));
    }


    [Theory]
    [InlineData(null)]
    public async Task GetSourcesAsync_Returns_Unencrypted_Expected_Content(string environment)
    {
        SymmetricEncryptor encryptor = new (SymmetricKey, SymmetricIv, new LogAdapter(null), null);
        EncryptedFileSourceProvider provider = new("ProviderName", "Core/Files", "Encrypted.Sources.json", new LogAdapter(null), environment, encryptor);
        IEnumerable<Source> actualSources = await provider.GetSourcesAsync(CancellationToken.None);

        byte[] content = await File.ReadAllBytesAsync(Path.Combine("Core/Files", "Encrypted.Sources.json"));
        content = encryptor.Decrypt(content);
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
