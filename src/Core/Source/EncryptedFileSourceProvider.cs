namespace Upgradier.Core;

public class EncryptedFileSourceProvider : FileSourceProvider
{
    private readonly IEncryptor _encryptor;

    public EncryptedFileSourceProvider(string name, string baseDirectory, string fileName, LogAdapter logger, string? environment, IEncryptor encryptor)
        : base(name, baseDirectory, fileName, logger, environment)
    {
        _encryptor = encryptor;
    }

    protected override async Task<byte[]> ReadFileAsync()
    {
        byte[] encryptedContent = await base.ReadFileAsync().ConfigureAwait(false);
        return _encryptor.Decrypt(encryptedContent);
    }
}