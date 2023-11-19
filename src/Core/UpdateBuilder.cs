using System.Net.Http;
using Microsoft.Extensions.Logging;
using Ugradier.Core;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private ISourceProvider? _sourceProvider;
    private IBatchStrategy? _batchStrategy;
    private IBatchCacheManager? _cacheManager;
    private IUpdateEvents? _events;
    private readonly List<IDatabaseEngine> _databaseEngines;
    private int _parallelism = 1;
    private ILogger _logger;

    public UpdateBuilder()
    {
        _databaseEngines = [];
    }

    public UpdateBuilder AddDatabaseEngines(params IDatabaseEngine[] databaseEngines)
    {
        ArgumentNullException.ThrowIfNull(databaseEngines);
        ArgumentOutOfRangeException.ThrowIfZero(databaseEngines.Length);
        _databaseEngines.AddRange(databaseEngines);
        return this;
    }

    public UpdateBuilder WithSourceProvider(ISourceProvider sourceProvider)
    {
        ArgumentNullException.ThrowIfNull(sourceProvider);
        _sourceProvider = sourceProvider;
        return this;
    }

    public UpdateBuilder WithBatchStrategy(IBatchStrategy batchStrategy)
    {
        ArgumentNullException.ThrowIfNull(batchStrategy);
        _batchStrategy = batchStrategy;
        return this;
    }

    public UpdateBuilder WithWebBatchStrategy(WebBatchStrategy batchStrategy, Func<HttpRequestMessage, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(batchStrategy);
        ArgumentNullException.ThrowIfNull(configureRequest);
        WebBatchStrategy webStrategy = batchStrategy;
        webStrategy.ConfigureRequestMessage(configureRequest);
        _batchStrategy = webStrategy;
        return this;
    }

    public UpdateBuilder WithCacheManager(IBatchCacheManager cacheManager)
    {
        ArgumentNullException.ThrowIfNull(cacheManager);
        _cacheManager = cacheManager;
        return this;
    }

    public UpdateBuilder WithFileCacheManager(string basePath)
    {
        DirectoryInfo directory = new (basePath);
        if(!directory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory {basePath} doesn't exists");
        }
        _cacheManager = new FileBatchCacheManager(basePath);
        return this;
    }

    public UpdateBuilder WithParallelism(int parallelism)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(parallelism, UpdateResultTaskBuffer.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parallelism, UpdateResultTaskBuffer.MaxValue);
        _parallelism = parallelism;
        return this;
    }

    public UpdateBuilder WithEvents(IUpdateEvents events)
    {
        ArgumentNullException.ThrowIfNull(events);
        _events = events;
        return this;
    }

    public UpdateBuilder WithLogger(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceProvider);
        ArgumentNullException.ThrowIfNull(_databaseEngines);
        ArgumentOutOfRangeException.ThrowIfZero(_databaseEngines.Count);
        ArgumentNullException.ThrowIfNull(_batchStrategy);

        return new UpdateManager(_sourceProvider
            , _databaseEngines.ToDictionary(databaseEngine => databaseEngine.Name)
            , _cacheManager is null ? _batchStrategy : new CachedBatchStrategy(_batchStrategy, _cacheManager)
            , new UpdateEventDispatcher(_events)
            , new LogAdapter(_logger)
            , _parallelism);
    }
}