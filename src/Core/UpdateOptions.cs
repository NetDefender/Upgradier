using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core
{
    [ExcludeFromCodeCoverage]
    public class UpdateOptions
    {
        public IEnumerable<Func<IDatabaseEngine>> DatabaseEngines { get; init; } = Enumerable.Empty<Func<IDatabaseEngine>>();

        public Func<ISourceProvider> SourceProvider { get; init; } = default!;

        public Func<IBatchStrategy> BatchStrategy { get; init; } = default!;

        public Func<IBatchCacheManager>? CacheManager { get; init; } = default!;
    }
}