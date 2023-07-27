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
    private int? _commandTimeout;
    private int? _connectionTimeout;

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
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(iv);
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
        if(environment?.Length == 0)
        {
            throw new ArgumentException(null, nameof(environment));
        }
        _environment = environment;
        return this;
    }

    public UpdateBuilder WithCommandTimeout(int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(seconds, 0);
        _commandTimeout = seconds;
        return this;
    }

    public UpdateBuilder WithConnectionTimeout(int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(seconds, 0);
        _connectionTimeout = seconds;
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceProviderFactory);
        ArgumentOutOfRangeException.ThrowIfZero(_databaseEnginesFactories.Count);
        ArgumentNullException.ThrowIfNull(_batchStrategyFactory);

        LogAdapter logger = new (_logger);

        logger.LogBuildingDatabaseEngines();
        DatabaseEngineCreationOptions databaseOptions = new() {  Logger = logger, Environment = _environment, CommandTimeout = _commandTimeout, ConnectionTimeout = _connectionTimeout };
        Dictionary<string, IDatabaseEngine> databaseEngines = _databaseEnginesFactories.Select(factory => factory(databaseOptions)).ToDictionary(engine => engine.Name);
        foreach (IDatabaseEngine databaseEngine in databaseEngines.Values)
        {
            ArgumentNullException.ThrowIfNull(databaseEngine);
        }

        logger.LogBuildingEvents();
        UpdateEventsCreationOptions updateEventsOptions = new () { Logger = logger, Environment = _environment };
        IUpdateEvents? events = _eventsFactory?.Invoke(updateEventsOptions);

        logger.LogBuildingBatchStrategy();
        BatchStrategyCreationOptions batchStrategyCreationOptions = new () { Logger = logger, Environment = _environment };
        IBatchStrategy batchStrategy = _batchStrategyFactory(batchStrategyCreationOptions);
        ArgumentNullException.ThrowIfNull(batchStrategy);

        logger.LogBuildingCacheManager();
        BatchCacheManagerCreationOptions cacheOptions = new() { Logger = logger, Environment = _environment };
        IBatchCacheManager? batchCacheManager = _cacheManagerFactory?.Invoke(cacheOptions);

        logger.LogBuildingEncryptor();
        EncryptionCreationOptions encryptorOptions = new() { Logger = logger, Environment = _environment };
        IEncryptor? encryptor = _encryptorFactory?.Invoke(encryptorOptions);

        logger.LogBuildingSourceProvider();
        SourceProviderCreationOptions sourceCreationOptions = new() { Logger = logger, Environment = _environment, Encryptor = encryptor };
        ISourceProvider sourceProvider = _sourceProviderFactory(sourceCreationOptions);
        ArgumentNullException.ThrowIfNull(sourceProvider);

        logger.LogBuildingUpdateManager();

        return new UpdateManager(sourceProvider
            , databaseEngines
            , batchCacheManager is null ? batchStrategy : new CachedBatchStrategy(batchStrategy, batchCacheManager, logger, _environment)
            , new UpdateEventDispatcher(events, logger, _environment)
            , logger
            , _parallelism
            , _environment);
    }
}