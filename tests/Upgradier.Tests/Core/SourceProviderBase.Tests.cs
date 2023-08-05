using System;
using System.Reflection;
using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class SourceProviderBase_Tests
{
    [Fact]
    public void Ctor_Throws_ArgumentNullException_If_Name_Is_Null()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>(null, null));
        Assert.True(exception?.GetBaseException() is ArgumentNullException);
    }

    [Fact]
    public void Ctor_Throws_ArgumentException_If_Name_Is_Empty()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>(null, string.Empty));
        Assert.True(exception?.GetBaseException() is ArgumentException);
    }

    [Theory]
    [InlineData("Dev", "SqlServer")]
    [InlineData(null, "Oracle")]
    public void Properties_In_Ctor_Are_Setted(string? environment, string name)
    {
        SourceProviderBase provider = Substitute.For<SourceProviderBase>(environment, name);
        Assert.Equal(environment, provider.Environment);
        Assert.Equal(name, provider.Name);
    }
}
