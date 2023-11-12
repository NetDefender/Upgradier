using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core;

[ExcludeFromCodeCoverage]
public record class UpdateResult(string Source, string DatabaseEngine, string ConnectionString, long OriginalVersion, long Version, Exception? Error);