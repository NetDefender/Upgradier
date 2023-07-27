namespace Upgradier.Core;

public class LockNotAdquiredException : Exception
{
    public LockNotAdquiredException() : base()
    {
    }

    public LockNotAdquiredException(string? message) : base(message)
    {
    }

    public LockNotAdquiredException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public static void ThrowIfNotAdquired(bool adquired, string? message = null)
    {
        if (!adquired)
        {
            throw new LockNotAdquiredException(message);
        }
    }
}