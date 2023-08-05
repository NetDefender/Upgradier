using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private int _waitTimeout;
    private Func<ISourceProvider>? _sourceProvider;
    private Func<IScriptStrategy>? _scriptStrategy;
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

    public UpdateBuilder WithScriptStrategy(Func<IScriptStrategy> scriptStrategy)
    {
        ArgumentNullException.ThrowIfNull(scriptStrategy);
        _scriptStrategy = scriptStrategy;
        return this;
    }

    public UpdateBuilder WithWebScriptStrategy(Func<WebScriptStrategy> scriptStrategy, Func<HttpRequestMessage, Task> configureRequest)
    {
        ArgumentNullException.ThrowIfNull(scriptStrategy);
        ArgumentNullException.ThrowIfNull(configureRequest);
        _scriptStrategy = () =>
        {
            WebScriptStrategy webStrategy = scriptStrategy();
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
        ArgumentNullException.ThrowIfNull(_scriptStrategy);
        ArgumentOutOfRangeException.ThrowIfZero(_providerFactories.Count);
        return new(new UpdateOptions
        {
            WaitTimeout = _waitTimeout,
            Providers = _providerFactories.AsReadOnly(),
            SourceProvider = _sourceProvider,
            ScriptStrategy = _scriptStrategy
        });
    }

    [ExcludeFromCodeCoverage]
    internal IEnumerable<Func<IProviderFactory>> GetProviderFactories()
    {
        return _providerFactories.AsReadOnly();
    }

    [ExcludeFromCodeCoverage]
    internal Func<IScriptStrategy>? GetScriptStrategy()
    {
        return _scriptStrategy;
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