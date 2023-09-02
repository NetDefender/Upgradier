using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private int _waitTimeout;
    private Func<ISourceProvider>? _sourceProvider;
    private Func<IBatchStrategy>? _batchStrategy;
    private readonly List<Func<IProviderFactory>> _providerFactories;

    public UpdateBuilder()
    {
        _providerFactories = new List<Func<IProviderFactory>>();
    }

    public UpdateBuilder AddProviderFactories(params Func<IProviderFactory>[] providerFactories)
    {
        ArgumentNullException.ThrowIfNull(providerFactories);
        ArgumentOutOfRangeException.ThrowIfZero(providerFactories.Length);
        _providerFactories.AddRange(providerFactories);
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

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceProvider);
        ArgumentNullException.ThrowIfNull(_batchStrategy);
        ArgumentOutOfRangeException.ThrowIfZero(_providerFactories.Count);
        return new(new UpdateOptions
        {
            WaitTimeout = _waitTimeout,
            Providers = _providerFactories.AsReadOnly(),
            SourceProvider = _sourceProvider,
            BatchStrategy = _batchStrategy
        });
    }

    [ExcludeFromCodeCoverage]
    internal IEnumerable<Func<IProviderFactory>> GetProviderFactories()
    {
        return _providerFactories.AsReadOnly();
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
    internal int GetWaitTimeout()
    {
        return _waitTimeout;
    }
}