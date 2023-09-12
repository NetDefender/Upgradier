namespace Ugradier.Core;

public class BatchCacheResult
{
    public static readonly BatchCacheResult Fail = new (false, null);

    public BatchCacheResult(bool success, string? contents)
    {
        Success = success;
        Contents = contents;
    }

    public bool Success { get; }

    public string? Contents { get; }
}
