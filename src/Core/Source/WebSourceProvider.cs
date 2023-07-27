using System.Net.Http;
using System.Net.Http.Json;

namespace Upgradier.Core;

public class WebSourceProvider : SourceProviderBase
{
    private readonly Uri _baseUri;
    private readonly string _baseFileName;
    private static readonly HttpClient _client = new();
    private Func<HttpRequestMessage, Task> _configureRequest;

    public WebSourceProvider(string name, Uri baseUri, string baseFileName, Func<HttpRequestMessage, Task>? configureRequest, LogAdapter logger, string? environment)
        : base(name, logger, environment)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        baseUri.ThrowIfIsNotAbsoluteUri();
        _baseUri = baseUri;
        _baseFileName = baseFileName;
        _configureRequest = configureRequest ?? (_ => Task.CompletedTask);
    }

    public override async Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        string rawFileName = Path.GetFileNameWithoutExtension(_baseFileName);
        string extension = Path.GetExtension(_baseFileName);
        string composedFileName = _baseFileName;
        if (!Environment.IsNullOrEmptyOrWhiteSpace())
        {
            composedFileName = $"{rawFileName}.{Environment}{extension}";
        }
        UriBuilder builder = new(_baseUri);
        string batchesUri = builder
            .CombinePath(composedFileName)
            .Uri.AbsoluteUri;
        Logger.LogSourcesFilePath(batchesUri);
        using HttpRequestMessage request = new(HttpMethod.Get, batchesUri);
        await _configureRequest(request).ConfigureAwait(false);
        using HttpResponseMessage response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        List<Source>? sources = await response.Content.ReadFromJsonAsync<List<Source>>(cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(sources);
        return sources.AsReadOnly().AsEnumerable();
    }

    public void ConfigureRequestMessage(Func<HttpRequestMessage, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(configureRequest);
        _configureRequest = configureRequest;
    }
}
