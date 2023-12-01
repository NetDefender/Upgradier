using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class LockManagerBaseTests
{
    [Fact]
    public void Ctor_Throws_When_SourceDatabase_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            try
            {
                Substitute.For<LockManagerBase>(null, new LogAdapter(null), null);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        });
    }

    [Fact]
    public void Ctor_Not_Throws_When_Environment_Is_Null()
    {
        LockManagerBase manager = Substitute.For<LockManagerBase>(null, new LogAdapter(null), null);
        Assert.Null(manager.Environment);
    }

    [Fact]
    public void Ctor_Not_Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            try
            {
                Substitute.For<LockManagerBase>(null, null, null);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        });
    }
}
