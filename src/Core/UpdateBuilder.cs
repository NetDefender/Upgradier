namespace Upgradier.Core;

public sealed class UpdateBuilder
{
    private int _waitTimeout;
    private Func<ISourceAdapter>? _sourceAdapter;
    private Func<IScriptAdapter>? _scriptAdapter;
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

    public UpdateBuilder WithSourceAdapter(Func<ISourceAdapter> sourceAdapter)
    {
        _sourceAdapter = sourceAdapter;
        return this;
    }

    public UpdateBuilder WithScriptAdapter(Func<IScriptAdapter> scriptAdapter)
    {
        _scriptAdapter = scriptAdapter;
        return this;
    }

    public UpdateBuilder WithWaitTimeout(int timeout)
    {
        _waitTimeout = timeout;
        return this;
    }

    public UpdateManager Build()
    {
        ArgumentNullException.ThrowIfNull(_sourceAdapter);
        ArgumentNullException.ThrowIfNull(_scriptAdapter);
        return new(new UpdateOptions
        {
            WaitTimeout = _waitTimeout,
            Providers = _providerFactories,
            SourceAdapter = _sourceAdapter,
            ScriptAdapter = _scriptAdapter
        });
    }
}