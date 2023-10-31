using System.Reflection;
using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class SourceProviderBase_Tests
{
    [Fact]
    public void Ctor_Throws_ArgumentNullException_If_Name_Is_Null()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>(null));
        Assert.True(exception?.GetBaseException() is ArgumentNullException);
    }

    [Fact]
    public void Ctor_Throws_ArgumentException_If_Name_Is_Empty()
    {
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => Substitute.For<SourceProviderBase>(string.Empty));
        Assert.True(exception?.GetBaseException() is ArgumentException);
    }
}
