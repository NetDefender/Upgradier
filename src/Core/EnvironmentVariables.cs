namespace Ugradier.Core;

public static class EnvironmentVariables
{
    public const string UPGRADIER_ENV_NAME = "UPGRADIER_ENV";

    public const string UPGRADIER_ENV_DEV = "Dev";

    public const string UPGRADIER_ENV_PROD = "Pro";

    public const string UPGRADIER_ENV_PRE = "Pre";

    public static string GetExecutionEnvironment()
    {
        return Environment.GetEnvironmentVariable(UPGRADIER_ENV_NAME);
    }

    public static void SetExecutionEnvironment(string? value)
    {
        Environment.SetEnvironmentVariable(UPGRADIER_ENV_NAME, value);
    }
}
