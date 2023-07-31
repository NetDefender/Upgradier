using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace Upgradier.Core;

public class WebScriptStrategy : ScriptStrategyBase
{
    private readonly string _baseUri;
    private static readonly HttpClient _client = new ();

    public WebScriptStrategy(string baseUri, string provider, string? environment) : base(environment, provider, nameof(FileScriptStrategy))
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        CoreExtensions.ThrowIfDirectoryNotExists(baseUri);
        _baseUri = baseUri;
    }

    public override async ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken)
    {
        string scriptsFile = Path.Combine(_baseUri, Provider, string.IsNullOrEmpty(Environment) ? "Index.json" : $"Index.{Environment}.json");
        List<Script>? scripts = await _client.GetFromJsonAsync<List<Script>>(scriptsFile, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(scripts);
        ArgumentOutOfRangeException.ThrowIfZero(scripts.Count);
        return scripts.AsReadOnly().AsEnumerable();
    }

    public override async ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder = new(_baseUri);
        StringBuilder uri = new StringBuilder(uriBuilder.Path, uriBuilder.Path.Length + 30).TrimEnd('/')
            .Append('/').Append(Provider)
            .Append('/').Append(script.VersionId).Append(".sql");
        uriBuilder.Path = uri.ToString();
        Stream stream = await _client.GetStreamAsync(uriBuilder.Uri, cancellationToken).ConfigureAwait(false);
        return new StreamReader(stream, leaveOpen: false);
    }
}
