using Microsoft.Extensions.Logging;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private Func<SourceProviderCreationOptions, ISourceProvider>? _sourceProviderFactory;
    private Func<BatchStrategyCreationOptions, IBatchStrategy>? _batchStrategyFactory;
    private Func<BatchCacheManagerCreationOptions, IBatchCacheManager>? _cacheManagerFactory;
    private Func<UpdateEventsCreationOptions, IUpdateEvents>? _eventsFactory;
    private Func<EncryptionCreationOptions, IEncryptor>? _encryptorFactory;
    private readonly List<Func<DatabaseEngineCreationOptions, IDatabaseEngine>> _databaseEnginesFactories;
    private int _parallelism = 1;
    private ILogger _logger;
    private string? _environment;

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

    public UpdateBuilder WithFileSourceProvider(string baseDirectory, string baseFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        ArgumentException.ThrowIfNullOrEmpty(baseFileName);
        _sourceProviderFactory = options => new FileSourceProvider(nameof(FileSourceProvider), baseDirectory, baseFileName, options.Logger, options.Environment);
        return this;
    }

    public UpdateBuilder WithEncryptedFileSourceProvider(string baseDirectory, string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        _sourceProviderFactory = options => new EncryptedFileSourceProvider(nameof(FileSourceProvider), baseDirectory, fileName, options.Logger, options.Environment, options.Encryptor);
        return this;
    }

    public UpdateBuilder WithEncryptor(Func<EncryptionCreationOptions, IEncryptor> encryptorFactory)
    {
        ArgumentNullException.ThrowIfNull(encryptorFactory);
        _encryptorFactory = encryptorFactory;
        return this;
    }

    public UpdateBuilder WithSymmetricEncryptor(string key, string iv)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNullOrEmpty(iv);
        _encryptorFactory = options => new SymmetricEncryptor(key, iv, options.Logger, options.Environment);
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

    public UpdateBuilder WithEnvironment(string environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _environment = environment;
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceProviderFactory);
        ArgumentOutOfRangeException.ThrowIfZero(_databaseEnginesFactories.Count);
        ArgumentNullException.ThrowIfNull(_batchStrategyFactory);

        LogAdapter logAdapter = new (_logger);

        DatabaseEngineCreationOptions databaseOptions = new() {  Logger = logAdapter, Environment = _environment };
        Dictionary<string, IDatabaseEngine> databaseEngines = _databaseEnginesFactories.Select(factory => factory(databaseOptions)).ToDictionary(engine => engine.Name);
        foreach (IDatabaseEngine databaseEngine in databaseEngines.Values)
        {
            ArgumentNullException.ThrowIfNull(databaseEngine);
        }

        UpdateEventsCreationOptions updateEventsOptions = new () { Logger = logAdapter, Environment = _environment };
        IUpdateEvents? events = _eventsFactory?.Invoke(updateEventsOptions);

        BatchStrategyCreationOptions batchStrategyCreationOptions = new () { Logger = logAdapter, Environment = _environment };
        IBatchStrategy batchStrategy = _batchStrategyFactory(batchStrategyCreationOptions);
        ArgumentNullException.ThrowIfNull(batchStrategy);

        BatchCacheManagerCreationOptions cacheOptions = new() { Logger = logAdapter, Environment = _environment };
        IBatchCacheManager? batchCacheManager = _cacheManagerFactory?.Invoke(cacheOptions);

        EncryptionCreationOptions encryptorOptions = new() { Logger = logAdapter, Environment = _environment };
        IEncryptor? encryptor = _encryptorFactory?.Invoke(encryptorOptions);

        SourceProviderCreationOptions sourceCreationOptions = new() { Logger = logAdapter, Environment = _environment, Encryptor = encryptor };
        ISourceProvider sourceProvider = _sourceProviderFactory(sourceCreationOptions);
        ArgumentNullException.ThrowIfNull(sourceProvider);

        return new UpdateManager(sourceProvider
            , databaseEngines
            , batchCacheManager is null ? batchStrategy : new CachedBatchStrategy(batchStrategy, batchCacheManager, logAdapter, _environment)
            , new UpdateEventDispatcher(events, logAdapter, _environment)
            , logAdapter
            , _parallelism
            , _environment);
    }
}