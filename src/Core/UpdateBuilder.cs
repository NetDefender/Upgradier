using System.Net.Http;
using Microsoft.Extensions.Logging;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private Func<ISourceProvider>? _sourceProvider;
    private Func<IBatchStrategy>? _batchStrategy;
    private Func<IBatchCacheManager>? _cacheManager;
    private Func<IUpdateEvents>? _events;
    private readonly List<Func<IDatabaseEngine>> _databaseEngines;
    private int _parallelism = 1;
    private ILogger _logger;

    public UpdateBuilder()
    {
        _databaseEngines = [];
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

    public UpdateBuilder WithCacheManager(Func<IBatchCacheManager> cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    public UpdateBuilder WithFileCacheManager(string basePath)
    {
        _cacheManager = () => new FileBatchCacheManager(basePath);
        return this;
    }

    public UpdateBuilder WithParallelism(int parallelism)
    {
        _parallelism = parallelism;
        return this;
    }

    public UpdateBuilder WithEvents(Func<IUpdateEvents> events)
    {
        _events = events;
        return this;
    }

    public UpdateBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceProvider);
        ArgumentNullException.ThrowIfNull(_batchStrategy);
        ArgumentNullException.ThrowIfNull(_batchStrategy);
        ArgumentOutOfRangeException.ThrowIfZero(_databaseEngines.Count);
        ArgumentOutOfRangeException.ThrowIfLessThan(_parallelism, UpdateResultTaskBuffer.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_parallelism, UpdateResultTaskBuffer.MaxValue);

        return new(new UpdateOptions
        {
            DatabaseEngines = _databaseEngines.AsReadOnly(),
            SourceProvider = _sourceProvider,
            BatchStrategy = _batchStrategy,
            CacheManager = _cacheManager,
            Parallelism = _parallelism,
            Events = _events,
            Logger = _logger
        });
    }
}