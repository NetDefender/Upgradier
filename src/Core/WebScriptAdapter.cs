using System.Net.Http;
using System.Net.Http.Json;

namespace Upgradier.Core;

public class WebScriptAdapter : ScriptAdapterBase
{
    private readonly string _baseUri;
    private static readonly HttpClient _client = new ();

    public WebScriptAdapter(string baseUri, string provider, string? environment) : base(environment, provider, nameof(FileScriptAdapter))
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        CoreExtensions.ThrowIfDirectoryNotExists(baseUri);
        _baseUri = baseUri;
    }

    public override async ValueTask<IEnumerable<Script>> GetAllScriptsAsync(CancellationToken cancellationToken)
    {
        string scriptsFile = Path.Combine(_baseUri, Provider, string.IsNullOrEmpty(Environment) ? "Scripts.json" : $"Scripts.{Environment}.json");
        List<Script>? scripts = await _client.GetFromJsonAsync<List<Script>>(scriptsFile, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(scripts);
        ArgumentOutOfRangeException.ThrowIfZero(scripts.Count);
        return scripts.AsReadOnly().AsEnumerable();
    }

    public override async ValueTask<StreamReader> GetScriptContentsAsync(Script script, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder = new(_baseUri);
        uriBuilder.Path ??= string.Empty;
        if (Environment is not null)
        {
            uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + $"/Scripts/{Environment}";
        }
        else
        {
            uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + "/Scripts";
        }
        uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + $"{script.VersionId}.sql";
        Stream stream = await _client.GetStreamAsync(uriBuilder.Uri, cancellationToken).ConfigureAwait(false);
        return new StreamReader(stream, leaveOpen: false);
    }
}
