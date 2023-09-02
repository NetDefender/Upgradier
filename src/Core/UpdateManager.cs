using Microsoft.EntityFrameworkCore;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly int _waitTimeout;
    private readonly ISourceProvider _sourceProvider;
    private readonly IBatchStrategy? _batchStrategy;
    private readonly IDictionary<string, IProviderFactory> _providerFactories;
    private readonly SemaphoreSlim _globalLock;
    private bool _isDisposed;

    public UpdateManager(UpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Providers);
        ArgumentOutOfRangeException.ThrowIfNegative(options.WaitTimeout);
        ArgumentOutOfRangeException.ThrowIfZero(options.Providers.Count());
        ArgumentNullException.ThrowIfNull(options.SourceProvider);

        _waitTimeout = options.WaitTimeout;
        _providerFactories = options.Providers
            .Select(providerFactory => providerFactory()!)
            .ToDictionary(factory => factory.Name);
        _sourceProvider = options.SourceProvider();
        _batchStrategy = options.BatchStrategy?.Invoke();
        _globalLock = new SemaphoreSlim(1, 1);
    }

    public async ValueTask<IEnumerable<UpdateResult>> Update(CancellationToken cancellationToken = default)
    {
        bool isGlobalLockAdquired = false;
        List<UpdateResult> results = new();
        try
        {
            if (await _globalLock.WaitAsync(_waitTimeout, cancellationToken).ConfigureAwait(false))
            {
                isGlobalLockAdquired = true;
                IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);

                foreach (Source source in sources)
                {
                    UpdateResult result = await UpdateSource(source, cancellationToken).ConfigureAwait(false);
                    results.Add(result);
                }
            }
        }
        finally
        {
            if (isGlobalLockAdquired)
            {
                _globalLock.Release();
            }
        }
        return results.AsReadOnly();
    }

    public async ValueTask<UpdateResult> UpdateSource(string sourceName, CancellationToken cancellationToken = default)
    {
        IEnumerable<Source> sources = await _sourceProvider.GetSourcesAsync(cancellationToken).ConfigureAwait(false);
        Source source = sources.First(s => s.Name == sourceName);
        return await UpdateSource(source, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<UpdateResult> UpdateSource(Source source, CancellationToken cancellationToken = default)
    {
        IProviderFactory factory = _providerFactories[source.Provider];
        IBatchStrategy batchStrategy = _batchStrategy ?? factory.CreateBatchStrategy();
        IEnumerable<Batch> batches = await batchStrategy.GetBatchesAsync(cancellationToken).ConfigureAwait(false);

        UpdateResult updateResult = new(source.Name, factory.Name, source.ConnectionString, -1, -1, null);
        try
        {
            using SourceDatabase sourceDatabase = factory.CreateSourceDatabase(source.ConnectionString);
            using ILockStrategy lockStrategy = factory.CreateLockStrategy(sourceDatabase);

            if (await lockStrategy.TryAdquireAsync(cancellationToken).ConfigureAwait(false))
            {
                DatabaseVersion currentVersion = await sourceDatabase.Version.FirstAsync(cancellationToken).ConfigureAwait(false);
                updateResult = updateResult with { OriginalVersion = currentVersion.VersionId };
                foreach (Batch batch in batches.Where(b => b.VersionId > currentVersion.VersionId).OrderBy(b => b.VersionId))
                {
                    using StreamReader sqlContentStream = await batchStrategy.GetBatchContentsAsync(batch, cancellationToken).ConfigureAwait(false);
                    string sqlContent = await sqlContentStream.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                    await sourceDatabase.Database.ExecuteSqlRawAsync(sqlContent, cancellationToken).ConfigureAwait(false);
                    currentVersion.VersionId = batch.VersionId;
                    updateResult = updateResult with { Version = batch.VersionId };
                    await sourceDatabase.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                await lockStrategy.FreeAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            updateResult = updateResult with { Version = updateResult.OriginalVersion, Error = ex };
        }

        return updateResult;
    }

    public bool IsUpdating()
    {
        return _globalLock.CurrentCount == 0;
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _globalLock.Dispose();
            }
            _isDisposed = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}