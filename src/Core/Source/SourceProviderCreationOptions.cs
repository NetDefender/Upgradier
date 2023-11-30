namespace Upgradier.Core;

public sealed class SourceProviderCreationOptions : BaseCreationOptions
{
    public IEncryptor Encryptor { get; init; }
}