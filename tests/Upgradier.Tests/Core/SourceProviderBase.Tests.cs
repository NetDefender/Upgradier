using System.Reflection;
using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class SourceProviderBase_Tests
{
    [Fact]
    public void Ctor_Throws_ArgumentNullException_When_Name_Is_Null()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>(null, new LogAdapter(null), null));
        Assert.True(exception?.GetBaseException() is ArgumentNullException);
    }

    [Fact]
    public void Ctor_Throws_ArgumentException_When_Name_Is_Empty()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>(string.Empty, new LogAdapter(null), null));
        Assert.True(exception?.GetBaseException() is ArgumentException);
    }

    [Fact]
    public void Ctor_Throws_ArgumentNullException_When_Logger_Is_Null()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>("A+", null, null));
        Assert.True(exception?.GetBaseException() is ArgumentNullException);
    }
}
