using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace Upgradier.Core;

public class WebScriptStrategy : ScriptStrategyBase
{
    private readonly Uri _baseUri;
    private static readonly HttpClient _client = new ();
    private Func<HttpRequestMessage, Task> _configureRequest = _ => Task.CompletedTask;

    public WebScriptStrategy(Uri baseUri, string provider, string? environment) : base(environment, provider, nameof(FileScriptStrategy))
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        CoreExtensions.ThrowIfIsNotAbsoluteUri(baseUri);
        _baseUri = baseUri;
    }

    public override async ValueTask<IEnumerable<Script>> GetScriptsAsync(CancellationToken cancellationToken)
    {
        UriBuilder builder = new (_baseUri);
        string scriptsUri = builder
            .CombinePath(string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json")
            .Uri.AbsoluteUri;
        using HttpRequestMessage request = new (HttpMethod.Get, scriptsUri);
        await _configureRequest(request).ConfigureAwait(false);
        using HttpResponseMessage response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        List<Script>? scripts = await response.Content.ReadFromJsonAsync<List<Script>>(cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(scripts);
        return scripts.AsReadOnly().AsEnumerable();
    }

    public override async ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder = new(_baseUri);
        StringBuilder uri = new StringBuilder(uriBuilder.Path, uriBuilder.Path.Length + 30).TrimEnd('/')
            .Append('/').Append(Provider)
            .AppendWhen(() => !string.IsNullOrEmpty(Environment), "/", Environment!)
            .Append('/').Append(script.VersionId).Append(".sql");
        uriBuilder.Path = uri.ToString();
        using HttpRequestMessage request = new(HttpMethod.Get, uriBuilder.Uri);
        await _configureRequest(request).ConfigureAwait(false);
        HttpResponseMessage response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return new StreamReader(stream, leaveOpen: false);
    }

    public void ConfigureRequestMessage(Func<HttpRequestMessage, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(configureRequest);
        _configureRequest = configureRequest;
    }
}
