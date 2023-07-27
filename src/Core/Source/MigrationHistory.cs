using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core;

[ExcludeFromCodeCoverage]
public sealed class MigrationHistory
{
    public int MigrationId { get; set; }
}
