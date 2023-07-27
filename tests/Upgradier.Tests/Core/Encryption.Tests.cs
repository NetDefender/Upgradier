using System.Text;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class Encryption_Tests
{
    private readonly byte[] SymmetricKey = [0xfa, 0x27, 0xc0, 0x47, 0xf7, 0xe6, 0xd8, 0x33, 0x52, 0x70, 0x35, 0x63, 0x08, 0x3c, 0xc2, 0xdd, 0x25, 0x7c, 0x63, 0xdd, 0x39, 0x1d, 0xe0, 0xba, 0xe2, 0x2c, 0xf8, 0x06, 0x16, 0x84, 0x48, 0xe8];
    private readonly byte[] SymmetricIv = [0x40, 0xba, 0x57, 0xe1, 0x63, 0x11, 0x75, 0x1c, 0xe0, 0x7d, 0x88, 0x3f, 0x6e, 0x53, 0xf5, 0x1b];

    [Fact]
    public void Symmetric_Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new SymmetricEncryptor(SymmetricKey, SymmetricIv, null, null));
    }

    [Fact]
    public void Symmetric_Throws_When_Key_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new SymmetricEncryptor(null, SymmetricIv, new LogAdapter(null), null));
    }


    [Fact]
    public void Symmetric_Throws_When_Iv_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new SymmetricEncryptor(SymmetricKey, null, new LogAdapter(null), null));
    }

    [Fact]
    public void Symmetric_Environment_Can_Be_Null()
    {
        new SymmetricEncryptor(SymmetricKey, SymmetricIv, new LogAdapter(null), null);
        new SymmetricEncryptor(SymmetricKey, SymmetricIv, new LogAdapter(null), "Dev");
    }

    [Fact]
    public void Symmetric_Environment_Can_Be_Not_Null()
    {
        SymmetricEncryptor encryptor = new (SymmetricKey, SymmetricIv, new LogAdapter(null), "Dev");
        Assert.Equal("Dev", encryptor.Environment);
    }

    [Fact]
    public void Symmetric_Encrypted_Message_Can_Be_Decrypted_And_Is_Equal_To_Original_Message()
    {
        SymmetricEncryptor encryptor = new (SymmetricKey, SymmetricIv, new LogAdapter(null), null);
        byte[] message = Encoding.UTF8.GetBytes("Hello, how are you");
        byte[] encrypted = encryptor.Encrypt(message);
        byte[] decrypted = encryptor.Decrypt(encrypted);
        Assert.True(message.SequenceEqual(decrypted));
    }

    [Fact]
    public void Supplying_Base64_Key_And_Iv_Results_In_The_Same_As_ByteArray()
    {
        SymmetricEncryptor encryptor64 = new(Convert.ToBase64String(SymmetricKey), Convert.ToBase64String(SymmetricIv), new LogAdapter(null), null);
        SymmetricEncryptor encryptor = new(SymmetricKey, SymmetricIv, new LogAdapter(null), null);

        byte[] message = Encoding.UTF8.GetBytes("Hello, how are you");
        byte[] encrypted = encryptor.Encrypt(message);
        byte[] encrypted64 = encryptor64.Encrypt(message);

        Assert.True(encrypted.SequenceEqual(encrypted64));

        byte[] derypted = encryptor.Decrypt(encrypted);
        byte[] decrypted64 = encryptor64.Decrypt(encrypted64);

        Assert.True(derypted.SequenceEqual(decrypted64));
    }
}
