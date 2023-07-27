using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Upgradier.Core;

public sealed class UpdateManager : IUpdateManager
{
    private readonly int _waitTimeout;
    private readonly ISourceAdapter _sourceAdapter;
    private readonly IScriptAdapter? _scriptAdapter;
    private readonly IDictionary<string, IProviderFactory> _providerFactories;
    private readonly SemaphoreSlim _globalLock;
    private bool _isDisposed;

    public UpdateManager(UpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Providers);
        ArgumentOutOfRangeException.ThrowIfNegative(options.WaitTimeout);
        ArgumentOutOfRangeException.ThrowIfZero(options.Providers.Count());
        ArgumentNullException.ThrowIfNull(options.SourceAdapter);

        _waitTimeout = options.WaitTimeout;
        _providerFactories = options.Providers
            .Select(providerFactory => providerFactory()!)
            .ToDictionary(factory => factory.Name);
        _sourceAdapter = options.SourceAdapter();
        _scriptAdapter = options.ScriptAdapter?.Invoke();
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
                IEnumerable<Source> sources = await _sourceAdapter.GetAllSourcesAsync(cancellationToken).ConfigureAwait(false);

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
        IEnumerable<Source> sources = await _sourceAdapter.GetAllSourcesAsync(cancellationToken).ConfigureAwait(false);
        Source? source = sources.FirstOrDefault(s => s.Name == sourceName);
        ArgumentNullException.ThrowIfNull(source);
        return await UpdateSource(source, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<UpdateResult> UpdateSource(Source source, CancellationToken cancellationToken = default)
    {
        IProviderFactory factory = _providerFactories[source.Provider];
        IScriptAdapter scriptAdapter = _scriptAdapter ?? factory.CreateScriptAdapter();
        IEnumerable<Script> scripts = await scriptAdapter.GetAllScriptsAsync(cancellationToken).ConfigureAwait(false);

        UpdateResult updateResult = new(source.Name, factory.Name, source.ConnectionString, -1, -1, null);
        try
        {
            using SourceDatabase sourceDatabase = factory.CreateSourceDatabase(source.ConnectionString);
            using ILockStrategy lockStrategy = factory.CreateLockStrategy(sourceDatabase);

            if (await lockStrategy.TryAdquireAsync(cancellationToken).ConfigureAwait(false))
            {
                DatabaseVersion currentVersion = await sourceDatabase.Version.FirstAsync(cancellationToken).ConfigureAwait(false);
                updateResult = updateResult with { OriginalVersion = currentVersion.VersionId };
                foreach (Script script in scripts.Where(s => s.VersionId > currentVersion.VersionId))
                {
                    using StreamReader sqlContentStream = await scriptAdapter.GetScriptContentsAsync(script, cancellationToken).ConfigureAwait(false);
                    string sqlContent = await sqlContentStream.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                    await sourceDatabase.Database.ExecuteSqlRawAsync(sqlContent, cancellationToken).ConfigureAwait(false);
                    currentVersion.VersionId = script.VersionId;
                    updateResult = updateResult with { Version = script.VersionId };
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