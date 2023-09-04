using System.Diagnostics.CodeAnalysis;
using Ugradier.Core;

namespace Upgradier.Core
{
    [ExcludeFromCodeCoverage]
    public class UpdateOptions
    {
        public int WaitTimeout { get; init; }
        public IEnumerable<Func<IProviderFactory>> Providers { get; init; } = Enumerable.Empty<Func<IProviderFactory>>();
        public Func<ISourceProvider> SourceProvider { get; init; } = default!;
        public Func<IBatchStrategy> BatchStrategy { get; init; } = default!;
        public Func<IBatchCacheManager>? CacheManager { get; init; } = default!;
    }
}