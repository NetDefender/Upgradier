using System.Net.Http;

namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private int _waitTimeout;
    private Func<ISourceProvider>? _sourceStrategy;
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

    public UpdateBuilder WithSourceProvider(Func<ISourceProvider> sourceAdapter)
    {
        _sourceStrategy = sourceAdapter;
        return this;
    }

    public UpdateBuilder WithScriptStrategy(Func<IScriptStrategy> scriptStrategy)
    {
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
        _waitTimeout = timeout;
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceStrategy);
        ArgumentNullException.ThrowIfNull(_scriptStrategy);
        return new(new UpdateOptions
        {
            WaitTimeout = _waitTimeout,
            Providers = _providerFactories,
            SourceAdapter = _sourceStrategy,
            ScriptAdapter = _scriptStrategy
        });
    }
}