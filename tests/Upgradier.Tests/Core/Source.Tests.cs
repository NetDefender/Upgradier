using Upgradier.Core;
using Xunit.Abstractions;

namespace Upgradier.Tests.Core;

public sealed class Source_Tests
{
    private readonly ITestOutputHelper _output;

    public Source_Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("NotImportant", "NotImportant", null)]
    [InlineData("NotImportant", null, "NotImportant")]
    [InlineData("NotImportant", null, null)]
    [InlineData(null, "NotImportant", "NotImportant")]
    [InlineData(null, "NotImportant", null)]
    [InlineData(null, null, "NotImportant")]
    [InlineData(null, null, null)]
    public void If_Any_Property_In_Source_Is_Null_Throws(string? name, string? provider, string? connectionString)
    {
        Assert.Throws<ArgumentNullException>(() => new Source(name, provider, connectionString));
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("", "", "NotImportant")]
    [InlineData("", "NotImportant", "")]
    [InlineData("", "NotImportant", "NotImportant")]
    [InlineData("NotImportant", "", "")]
    [InlineData("NotImportant", "", "NotImportant")]
    [InlineData("NotImportant", "NotImportant", "")]
    public void If_Any_Property_In_Source_Is_Empty_Throws(string name, string provider, string connectionString)
    {
        Assert.Throws<ArgumentException>(() => new Source(name, provider, connectionString));
    }
}