namespace Upgradier.Core;

public class BatchCacheResult
{
    public static readonly BatchCacheResult Miss = new (false, false, null);

    public static readonly BatchCacheResult Locked = new(false, true, null);

    public BatchCacheResult(bool success, bool isLocked, string? contents)
    {
        Success = success;
        IsLocked = isLocked;
        Contents = contents;
    }

    public bool Success { get; }

    public bool IsLocked { get; }

    public string? Contents { get; }
}
