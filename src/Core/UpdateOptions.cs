namespace Upgradier.Core
{
    public class UpdateOptions
    {
        public int WaitTimeout { get; init; }
        public IEnumerable<Func<IProviderFactory>> Providers { get; init; } = Enumerable.Empty<Func<IProviderFactory>>();
        public Func<ISourceAdapter> SourceAdapter { get; init; } = default!;
        public Func<IScriptAdapter> ScriptAdapter { get; init; } = default!;
    }
}