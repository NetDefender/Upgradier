using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core;

[ExcludeFromCodeCoverage]
public abstract class BaseCreationOptions
{
    public required LogAdapter Logger { get; init; }

    public string? Environment { get; init; }
}
