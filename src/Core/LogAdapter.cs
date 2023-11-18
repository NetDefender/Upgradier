using Microsoft.Extensions.Logging;
using Upgradier.Core;

namespace Ugradier.Core;

public sealed class LogAdapter
{
    private readonly ILogger? _logger;

    public LogAdapter(ILogger? logger)
    {
        _logger = logger;
    }

    public void LogResults(IEnumerable<UpdateResult> results)
    {
        if (results.Any(result => result.Error is not null))
        {
            foreach (UpdateResult resultWithError in results.Where(result => result.Error is not null))
            {
                _logger?.LogError("Execution error in source [{Source}] with initial version [{Version}] and connection string [{ConnectionString}]. Batch that produced error was [{ErrorVersion}] with message [{Message}]", resultWithError.Source, resultWithError.Version, resultWithError.ConnectionString, resultWithError.ErrorVersion, resultWithError.Error?.Message);
            }
            _logger?.LogError("Update completed with errors");
        }
        else
        {
            _logger?.LogInformation("Update completed with success!");
        }
    }

    public void LogSources(IEnumerable<Source> sources)
    {
        foreach (Source source in sources)
        {
            _logger?.LogDebug("Available source [{Name}] with provider [{Provider}] and connection string [{ConnectionString}]", source.Name, source.Provider, source.ConnectionString);
        }
    }

    public void LogStarting()
    {
        _logger?.LogInformation("Starting updating sources");
    }

    public IDisposable? LogBeginUpdatingSource(Source source)
    {
        return _logger?.BeginScope(source.Name);
    }

    public void LogUpdatingSource(Source source)
    {
        _logger?.LogDebug("Updating source [{Name}] with provider [{Provider}] and connection string [{ConnectionString}]", source.Name, source.Provider, source.ConnectionString);
    }

    public void LogGettingInitialSourceVersion(Source source, DatabaseVersion currentVersion)
    {
        _logger?.LogDebug("Getting initial version in [{Source}] for updating with value [{Version}]", source.Name, currentVersion.VersionId);
    }

    public void LogExecutingBatch(Source source, Batch batch, string batchContents)
    {
        _logger?.LogDebug("Executing batch in source [{Source}], batch [{VersionId}] with Contents: {BatchContent}", source.Name, batch.VersionId, batchContents);
    }

    public void LogUpdatingSourceError(Source source, Batch? batchWithError, Exception ex)
    {
        _logger?.LogError(ex, "An error ocurred updating the source [{Source}] with connection string [{ConnectionString}], provider [{Provider}] and batch [{Batch}]", source.Name, source.ConnectionString, source.Provider, batchWithError?.VersionId);
    }
}
