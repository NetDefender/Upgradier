using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core;

[ExcludeFromCodeCoverage]
public sealed class DatabaseEngineCreationOptions : BaseCreationOptions
{
    public int? CommandTimeout { get; init; }

    public int? ConnectionTimeout { get; init; }
}