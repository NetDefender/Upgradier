using Microsoft.Extensions.Logging;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private Func<SourceProviderCreationOptions, ISourceProvider>? _sourceProviderFactory;
    private Func<BatchStrategyCreationOptions, IBatchStrategy>? _batchStrategyFactory;
    private Func<BatchCacheManagerCreationOptions, IBatchCacheManager>? _cacheManagerFactory;
    private Func<UpdateEventsCreationOptions, IUpdateEvents>? _eventsFactory;
    private readonly List<Func<DatabaseEngineCreationOptions, IDatabaseEngine>> _databaseEnginesFactories;
    private int _parallelism = 1;
    private ILogger _logger;

    public UpdateBuilder()
    {
        _databaseEnginesFactories = [];
    }

    public UpdateBuilder AddDatabaseEngines(params Func<DatabaseEngineCreationOptions, IDatabaseEngine>[] databaseEnginesFactories)
    {
        ArgumentNullException.ThrowIfNull(databaseEnginesFactories);
        ArgumentOutOfRangeException.ThrowIfZero(databaseEnginesFactories.Length);
        _databaseEnginesFactories.AddRange(databaseEnginesFactories);
        return this;
    }

    public UpdateBuilder WithSourceProvider(Func<SourceProviderCreationOptions, ISourceProvider> sourceProviderFactory)
    {
        ArgumentNullException.ThrowIfNull(sourceProviderFactory);
        _sourceProviderFactory = sourceProviderFactory;
        return this;
    }

    public UpdateBuilder WithBatchStrategy(Func<BatchStrategyCreationOptions, IBatchStrategy> batchStrategyFactory)
    {
        ArgumentNullException.ThrowIfNull(batchStrategyFactory);
        _batchStrategyFactory = batchStrategyFactory;
        return this;
    }

    public UpdateBuilder WithWebBatchStrategy(Func<BatchStrategyCreationOptions, WebBatchStrategy> batchStrategyFactory)
    {
        ArgumentNullException.ThrowIfNull(batchStrategyFactory);
        _batchStrategyFactory = batchStrategyFactory;
        return this;
    }

    public UpdateBuilder WithCacheManager(Func<BatchCacheManagerCreationOptions, IBatchCacheManager> cacheManagerFactory)
    {
        ArgumentNullException.ThrowIfNull(cacheManagerFactory);
        _cacheManagerFactory = cacheManagerFactory;
        return this;
    }

    public UpdateBuilder WithParallelism(int parallelism)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(parallelism, UpdateResultTaskBuffer.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parallelism, UpdateResultTaskBuffer.MaxValue);
        _parallelism = parallelism;
        return this;
    }

    public UpdateBuilder WithEvents(Func<UpdateEventsCreationOptions, IUpdateEvents> eventsFactory)
    {
        ArgumentNullException.ThrowIfNull(eventsFactory);
        _eventsFactory = eventsFactory;
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
        ArgumentNullException.ThrowIfNull(_sourceProviderFactory);
        ArgumentOutOfRangeException.ThrowIfZero(_databaseEnginesFactories.Count);
        ArgumentNullException.ThrowIfNull(_batchStrategyFactory);

        LogAdapter logAdapter = new (_logger);

        DatabaseEngineCreationOptions databaseOptions = new() {  Logger = logAdapter };
        Dictionary<string, IDatabaseEngine> databaseEngines = _databaseEnginesFactories.Select(factory => factory(databaseOptions)).ToDictionary(engine => engine.Name);
        foreach (IDatabaseEngine databaseEngine in databaseEngines.Values)
        {
            ArgumentNullException.ThrowIfNull(databaseEngine);
        }

        UpdateEventsCreationOptions updateEventsOptions = new () { Logger = logAdapter };
        IUpdateEvents? events = _eventsFactory?.Invoke(updateEventsOptions);

        BatchStrategyCreationOptions batchStrategyCreationOptions = new () { Logger = logAdapter };
        IBatchStrategy batchStrategy = _batchStrategyFactory(batchStrategyCreationOptions);
        ArgumentNullException.ThrowIfNull(batchStrategy);

        BatchCacheManagerCreationOptions cacheOptions = new() { Logger = logAdapter };
        IBatchCacheManager? batchCacheManager = _cacheManagerFactory?.Invoke(cacheOptions);

        SourceProviderCreationOptions sourceCreationOptions = new() { Logger = logAdapter };
        ISourceProvider sourceProvider = _sourceProviderFactory(sourceCreationOptions);
        ArgumentNullException.ThrowIfNull(sourceProvider);

        return new UpdateManager(sourceProvider
            , databaseEngines
            , batchCacheManager is null ? batchStrategy : new CachedBatchStrategy(batchStrategy, batchCacheManager, logAdapter)
            , new UpdateEventDispatcher(events, logAdapter)
            , logAdapter
            , _parallelism);
    }
}