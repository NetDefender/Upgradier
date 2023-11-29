namespace Upgradier.Core;

public abstract class BaseCreationOptions
{
    public required LogAdapter Logger { get; init; }

    public string? Environment { get; init; }
}
