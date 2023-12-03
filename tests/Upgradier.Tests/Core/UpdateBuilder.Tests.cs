using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class UpdateBuilder_Tests
{

    [Fact]
    public void AddDatabaseEngines_Throws_When_Null_IDatabaseEngines()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.AddDatabaseEngines(null));
    }

    [Fact]
    public void AddDatabaseEngines_Throws_When_Empty_IDatabaseEngines()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddDatabaseEngines());
    }

    [Fact]
    public void WithSourceProvider_Throws_When_Null_SourceProviderFactory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithSourceProvider(null));
    }

    [Fact]
    public void WithFileSourceProvider_Throws_When_Null_Or_Empty_BaseDirectory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithFileSourceProvider(null, "File.txt"));
        Assert.Throws<ArgumentException>(() => builder.WithFileSourceProvider(string.Empty, "File.txt"));
    }

    [Fact]
    public void WithFileSourceProvider_Throws_When_Null_Or_Empty_BaseFile()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithFileSourceProvider("Directory/Example", null));
        Assert.Throws<ArgumentException>(() => builder.WithFileSourceProvider("Directory/Example", string.Empty));
    }

    [Fact]
    public void WithEncrypedFileSourceProvider_Throws_When_Null_Or_Empty_BaseDirectory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithEncryptedFileSourceProvider(null, "File.txt"));
        Assert.Throws<ArgumentException>(() => builder.WithEncryptedFileSourceProvider(string.Empty, "File.txt"));
    }

    [Fact]
    public void WithEncryptedFileSourceProvider_Throws_When_Null_Or_Empty_BaseFile()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithEncryptedFileSourceProvider("Directory/Example", null));
        Assert.Throws<ArgumentException>(() => builder.WithEncryptedFileSourceProvider("Directory/Example", string.Empty));
    }


    [Fact]
    public void WithEncryptor_Throws_When_Null_EncryptorFactory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithEncryptor(null));
    }

    [Fact]
    public void WithSymmetricEncryptor_Throws_When_Null_Or_Empty_Key()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithSymmetricEncryptor(null, "adsfasdfaew32423423"));
        Assert.Throws<ArgumentException>(() => builder.WithSymmetricEncryptor(string.Empty, "21312321dfdsfa"));
    }

    [Fact]
    public void WithSymmetricEncryptor_Throws_When_Null_Or_Empty_Iv()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithSymmetricEncryptor("adsfasdfaew32423423", null));
        Assert.Throws<ArgumentException>(() => builder.WithSymmetricEncryptor("21312321dfdsfa", string.Empty));
    }

    [Fact]
    public void WithBatchStrategy_Throws_When_Null_Factory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithBatchStrategy(null));
    }

    [Fact]
    public void WithWebBatchStrategy_Throws_When_Null_Factory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithWebBatchStrategy(null));
    }

    [Fact]
    public void WithCacheManager_Throws_When_Null_Factory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithCacheManager(null));
    }

    [Fact]
    public void WithParallelism_Throws_When_Value_Lesser_Than_UpdateResultTaskBuffer_MinValue()
    {
        int invalidParallelism = UpdateResultTaskBuffer.MinValue - 1;
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithParallelism(invalidParallelism));
    }

    [Fact]
    public void WithParallelism_Throws_When_Value_Greater_Than_UpdateResultTaskBuffer_MaxValue()
    {
        int invalidParallelism = UpdateResultTaskBuffer.MaxValue + 1;
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithParallelism(invalidParallelism));
    }

    [Fact]
    public void WithEvents_Throws_When_Null_Factory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithEvents(null));
    }

    [Fact]
    public void WithLogger_Throws_When_Null_Factory()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithLogger(null));
    }

    [Fact]
    public void WithEnvironment_Throws_When_Empty()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentException>(() => builder.WithEnvironment(string.Empty));
    }

    [Fact]
    public void WithEnvironment_Not_Throws_When_Null()
    {
        UpdateBuilder builder = new();
        builder.WithEnvironment(null);
    }

    [Fact]
    public void WithCommandTimeout_Throws_When_Less_Than_0()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithCommandTimeout(-1));
    }

    [Fact]
    public void WithConnectionTimeout_Throws_When_Less_Than_0()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithConnectionTimeout(-1));
    }
}
