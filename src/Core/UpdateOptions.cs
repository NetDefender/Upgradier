using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core
{
    [ExcludeFromCodeCoverage]
    public class UpdateOptions
    {
        public int WaitTimeout { get; init; }
        public IEnumerable<Func<IProviderFactory>> Providers { get; init; } = Enumerable.Empty<Func<IProviderFactory>>();
        public Func<ISourceProvider> SourceAdapter { get; init; } = default!;
        public Func<IScriptStrategy> ScriptAdapter { get; init; } = default!;
    }
}