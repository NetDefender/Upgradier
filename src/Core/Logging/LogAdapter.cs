using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Upgradier.Core;

public class LogAdapter
{
    private readonly ILogger? _logger;

    public LogAdapter(ILogger? logger)
    {
        _logger = logger;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogSources(IEnumerable<Source> sources)
    {
        foreach (Source source in sources)
        {
            _logger?.LogDebug("Available source [{Name}] with provider [{Provider}] and connection string [{ConnectionString}]", source.Name, source.Provider, source.ConnectionString);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogStarting()
    {
        _logger?.LogInformation("Starting updating sources");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IDisposable? LogBeginUpdatingSource(Source source)
    {
        return _logger?.BeginScope(source.Name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogUpdatingSource(Source source)
    {
        _logger?.LogDebug("Updating source [{Name}] with provider [{Provider}] and connection string [{ConnectionString}]", source.Name, source.Provider, source.ConnectionString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogGettingInitialSourceVersion(Source source, DatabaseVersion currentVersion)
    {
        _logger?.LogDebug("Getting initial version in [{Source}] for updating with value [{Version}]", source.Name, currentVersion.VersionId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogExecutingBatch(Source source, Batch batch, string batchContents)
    {
        _logger?.LogDebug("Executing batch in source [{Source}], batch [{VersionId}] with Contents: {BatchContent}", source.Name, batch.VersionId, batchContents);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogUpdatingSourceError(Source source, Batch? batchWithError, Exception ex)
    {
        _logger?.LogError(ex, "An error ocurred updating the source [{Source}] with connection string [{ConnectionString}], provider [{Provider}] and batch [{Batch}]", source.Name, source.ConnectionString, source.Provider, batchWithError?.VersionId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBatchCacheHit(long batchId)
    {
        _logger?.LogDebug("Batch [{BatchId}] from Cache", batchId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBatchCacheMiss(long batchId)
    {
        _logger?.LogDebug("Batch [{BatchId}] missed Cache", batchId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBatchCacheStore(long batchId, bool stored)
    {
        _logger?.LogDebug("Batch [{BatchId}] stored [{Stored}] in Cache", batchId, stored);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBeforeBatchProcessingAsyncDispatchEvent(long version)
    {
        _logger?.LogDebug("Dispatching event [{Event}] with version [{Version}] ", nameof(IUpdateEvents.BeforeBatchProcessingAsync), version);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogAfterBatchProcessedAsyncDispatchEvent(long version)
    {
        _logger?.LogDebug("Dispatching event [{Version}] stored [{Event}]", nameof(IUpdateEvents.AfterBatchProcessedAsync), version);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogTaskBufferAddSuccessfully(int count)
    {
        _logger?.LogTrace("Task added to buffer with [{Count}] total tasks",count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogTaskBufferFull(int count)
    {
        _logger?.LogTrace("Task buffere is full with [{Count}] total tasks", count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogChangingCurrentVersion(Source source, long currentVersion, long newVersion)
    {
        _logger?.LogDebug("Incrementing version of source [{Source}] from [{CurrentVersion}] to [{NewVersion}]", source.Name,  currentVersion, newVersion);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogGetBatchesPath(string file)
    {
        _logger?.LogDebug("Getting batches from [{File}]", file);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogGetBatchesArray(List<Batch> batches)
    {
        foreach (Batch batch in batches)
        {
            _logger?.LogDebug("Batch detected with version [{Version}]", batch.VersionId);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogGetBatchFilePath(string batchFile)
    {
        _logger?.LogDebug("Reading batch file [{File}]", batchFile);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogGetBatchUri(Uri uri)
    {
        _logger?.LogDebug("Reading batch file from uri [{Uri}]", uri);
    }
}
