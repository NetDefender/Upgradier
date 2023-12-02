using System.Security.Cryptography;

namespace Upgradier.Core;

public class SymmetricEncryptor : IEncryptor
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public SymmetricEncryptor(byte[] key, byte[] iv, LogAdapter logger, string? environment)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(iv);
        ArgumentNullException.ThrowIfNull(logger);
        _key = key;
        _iv = iv;
        Logger = logger;
        Environment = environment;
    }

    public SymmetricEncryptor(string key, string iv, LogAdapter logger, string environment)
        : this(Convert.FromBase64String(key), Convert.FromBase64String(iv), logger, environment)
    {
        //Examples "+ifAR/fm2DNScDVjCDzC3SV8Y905HeC64iz4BhaESOg=", "QLpX4WMRdRzgfYg/blP1Gw=="
    }

    public LogAdapter Logger { get; }

    public string? Environment { get; }

    public byte[] Encrypt(byte[] data)
    {
        using Aes aes = Aes.Create();
        aes.IV = _iv;
        aes.Key = _key;

        return aes.EncryptCbc(data, _iv, PaddingMode.PKCS7);
    }

    public byte[] Decrypt(byte[] data)
    {
        using Aes aes = Aes.Create();
        aes.IV = _iv;
        aes.Key = _key;

        return aes.DecryptCbc(data, _iv, PaddingMode.PKCS7);
    }
}