namespace Upgradier.Core;

public sealed class DatabaseEngineCreationOptions : BaseCreationOptions
{
    public int? CommandTimeout { get; init; }
}