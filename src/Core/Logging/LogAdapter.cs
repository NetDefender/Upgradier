using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Upgradier.Core;

[ExcludeFromCodeCoverage]
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
                _logger?.LogError("Execution error in source [{Source}] with initial version [{Version}]. Batch that produced error was [{ErrorVersion}] with message [{Message}]", resultWithError.Source, resultWithError.Version, resultWithError.ErrorVersion, resultWithError.Error?.Message);
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
            _logger?.LogDebug("Available source [{Name}] with provider [{Provider}]", source.Name, source.Provider);
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
        _logger?.LogDebug("Updating source [{Name}] with provider [{Provider}]", source.Name, source.Provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogGettingInitialSourceVersion(Source source, DatabaseVersion currentVersion)
    {
        _logger?.LogDebug("Getting initial version in [{Source}] for updating with value [{Version}]", source.Name, currentVersion.VersionId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogExecuteBatch(string batchContents)
    {
        _logger?.LogDebug("Executing batch with Contents: {BatchContent}", batchContents);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogUpdatingSourceError(Source source, long? lastVersion, Exception ex)
    {
        _logger?.LogError(ex, "An error ocurred updating the source [{Source}], provider [{Provider}]. Last good version was [{Version}]", source.Name, source.Provider, lastVersion);
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
    public void LogBatchCacheStored(long batchId)
    {
        _logger?.LogDebug("Batch [{BatchId}] stored in Cache", batchId);
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
        _logger?.LogTrace("Task added to buffer with [{Count}] total tasks", count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogTaskBufferFull(int count)
    {
        _logger?.LogTrace("Task buffere is full with [{Count}] total tasks", count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogChangingCurrentVersion(long currentVersion, long newVersion)
    {
        _logger?.LogDebug("Incrementing version from [{CurrentVersion}] to [{NewVersion}]", currentVersion, newVersion);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogTryStoreError(long versionId, string provider, Exception ex)
    {
        _logger?.LogError(ex, "Error storing cache for batch with version [{VersionId}] and provider [{Provider}]. ", versionId, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogTryLoadError(long versionId, string provider, Exception ex)
    {
        _logger?.LogError(ex, "Error loading cache for batch with version [{VersionId}] and provider [{Provider}]. ", versionId, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogEncrypting()
    {
        _logger?.LogDebug("Encrypting data");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogDecrypting()
    {
        _logger?.LogDebug("Decrypting data");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogSourcesFilePath(string fullPath)
    {
        _logger?.LogDebug("Getting sources from [{Path}]", fullPath);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogSourceModelCreating()
    {
        _logger?.LogDebug("Creating EntityFramework Model");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogChangedCurrentVersion(long newVersion)
    {
        _logger?.LogDebug("Version incremented to [{NewVersion}]", newVersion);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogEnsuringSchema()
    {
        _logger?.LogDebug("Ensuring schema");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogExecutingMigration(int migrationId)
    {
        _logger?.LogDebug("Executing migration [{MigrationId}]", migrationId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingDatabaseEngines() 
    {
        _logger?.LogDebug("Building Database Engines");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingEvents() 
    {
        _logger?.LogDebug("Building Update Events");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingCacheManager()
    {
        _logger?.LogDebug("Building Cache Manager");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingBatchStrategy()
    {
        _logger?.LogDebug("Building Batch Strategy");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingEncryptor() 
    {
        _logger?.LogDebug("Building Encryptor");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingSourceProvider() 
    {
        _logger?.LogDebug("Building Source Provider");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogBuildingUpdateManager() 
    {
        _logger?.LogDebug("Building Update Manager");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogCreatingLockStrategy(string strategy)
    {
        _logger?.LogDebug("Creating Strategy [{Strategy}]", strategy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogCreatingSourceDatabase(string sourceDatabase)
    {
        _logger?.LogDebug("Creating Source Database [{SourceDatabase}]", sourceDatabase);
    }
}