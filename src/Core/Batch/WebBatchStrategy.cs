using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace Upgradier.Core;

public class WebBatchStrategy : BatchStrategyBase
{
    private readonly Uri _baseUri;
    private static readonly HttpClient _client = new ();
    private Func<HttpRequestMessage, Task> _configureRequest;

    public WebBatchStrategy(Uri baseUri, Func<HttpRequestMessage, Task>? configureRequest, LogAdapter logger) : base(nameof(FileBatchStrategy), logger)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        baseUri.ThrowIfIsNotAbsoluteUri();
        _baseUri = baseUri;
        _configureRequest = configureRequest ?? (_ => Task.CompletedTask);
    }

    public override async Task<IEnumerable<Batch>> GetBatchesAsync(CancellationToken cancellationToken)
    {
        UriBuilder builder = new (_baseUri);
        string batchesUri = builder
            .CombinePath(string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json")
            .Uri.AbsoluteUri;
        using HttpRequestMessage request = new (HttpMethod.Get, batchesUri);
        await _configureRequest(request).ConfigureAwait(false);
        using HttpResponseMessage response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        List<Batch>? batches = await response.Content.ReadFromJsonAsync<List<Batch>>(cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(batches);
        Logger.LogGetBatchesArray(batches);
        return batches.AsReadOnly().AsEnumerable();
    }

    public override async Task<string> GetBatchContentsAsync(Batch batch, string provider, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder = new(_baseUri);
        StringBuilder uri = new StringBuilder(uriBuilder.Path, uriBuilder.Path.Length + 30).TrimEnd('/')
            .Append('/').Append(provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(batch.VersionId).Append(".sql");
        uriBuilder.Path = uri.ToString();
        Logger.LogGetBatchUri(uriBuilder.Uri);
        using HttpRequestMessage request = new(HttpMethod.Get, uriBuilder.Uri);
        await _configureRequest(request).ConfigureAwait(false);
        using HttpResponseMessage response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public void ConfigureRequestMessage(Func<HttpRequestMessage, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(configureRequest);
        _configureRequest = configureRequest;
    }
}
