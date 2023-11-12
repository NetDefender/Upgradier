using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class LockStrategyBase_Tests
{
    [Fact]
    public void Ctor_Throws_If_SourceDatabase_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            try
            {
                Substitute.For<LockStrategyBase>(new object[] { null! });
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        });
    }
}
