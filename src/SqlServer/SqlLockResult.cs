namespace Upgradier.SqlServer;

public enum SqlLockResult
{
    Success = 0,
    Granted = 1,
    Timeout = -1,
    Canceled = -2,
    Deadlock = -3,
    ValidationError = -999
}
