using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private int _waitTimeout;
    private Func<ISourceProvider>? _sourceProvider;
    private Func<IBatchStrategy>? _batchStrategy;
    private Func<IBatchCacheManager>? _cacheManager;
    private readonly List<Func<IDatabaseEngine>> _databaseEngines;

    public UpdateBuilder()
    {
        _databaseEngines = new List<Func<IDatabaseEngine>>();
    }

    public UpdateBuilder AddDatabaseEngines(params Func<IDatabaseEngine>[] databaseEngines)
    {
        ArgumentNullException.ThrowIfNull(databaseEngines);
        ArgumentOutOfRangeException.ThrowIfZero(databaseEngines.Length);
        _databaseEngines.AddRange(databaseEngines);
        return this;
    }

    public UpdateBuilder WithSourceProvider(Func<ISourceProvider> sourceProvider)
    {
        ArgumentNullException.ThrowIfNull(sourceProvider);
        _sourceProvider = sourceProvider;
        return this;
    }

    public UpdateBuilder WithBatchStrategy(Func<IBatchStrategy> batchStrategy)
    {
        ArgumentNullException.ThrowIfNull(batchStrategy);
        _batchStrategy = batchStrategy;
        return this;
    }

    public UpdateBuilder WithWebBatchStrategy(Func<WebBatchStrategy> batchStrategy, Func<HttpRequestMessage, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(batchStrategy);
        ArgumentNullException.ThrowIfNull(configureRequest);
        _batchStrategy = () =>
        {
            WebBatchStrategy webStrategy = batchStrategy();
            webStrategy.ConfigureRequestMessage(configureRequest);
            return webStrategy;
        };
        return this;
    }

    public UpdateBuilder WithWaitTimeout(int timeout)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timeout);
        _waitTimeout = timeout;
        return this;
    }

    public UpdateBuilder WithCacheManager(Func<IBatchCacheManager> cacheManager)
    {
        ArgumentNullException.ThrowIfNull(cacheManager);
        _cacheManager = cacheManager;
        return this;
    }

    public UpdateBuilder WithFileCacheManager(string basePath)
    {
        _cacheManager = () => new FileBatchCacheManager(basePath);
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceProvider);
        ArgumentNullException.ThrowIfNull(_batchStrategy);
        ArgumentOutOfRangeException.ThrowIfZero(_databaseEngines.Count);

        return new(new UpdateOptions
        {
            WaitTimeout = _waitTimeout,
            DatabaseEngines = _databaseEngines.AsReadOnly(),
            SourceProvider = _sourceProvider,
            BatchStrategy = _batchStrategy,
            CacheManager = _cacheManager
        });
    }

    [ExcludeFromCodeCoverage]
    internal IEnumerable<Func<IDatabaseEngine>> GetDatabaseEngines()
    {
        return _databaseEngines.AsReadOnly();
    }

    [ExcludeFromCodeCoverage]
    internal Func<IBatchStrategy>? GetBatchStrategy()
    {
        return _batchStrategy;
    }

    [ExcludeFromCodeCoverage]
    internal Func<ISourceProvider>? GetSourceProvider()
    {
        return _sourceProvider;
    }

    [ExcludeFromCodeCoverage]
    internal Func<IBatchCacheManager>? GetCacheManager()
    {
        return _cacheManager;
    }

    [ExcludeFromCodeCoverage]
    internal int GetWaitTimeout()
    {
        return _waitTimeout;
    }
}