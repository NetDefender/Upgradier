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
                Substitute.For<LockStrategyBase>("dev", null);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        });
    }

    [Fact]
    public void Environment_Is_Assigned()
    {
        const string ENVIRONMENT = "Dev";
        SourceDatabase sourceDatabase = Substitute.For<SourceDatabase>(ENVIRONMENT, new DbContextOptionsBuilder().Options);
        LockStrategyBase strategy = Substitute.For<LockStrategyBase>(ENVIRONMENT, sourceDatabase);
        Assert.Equal(ENVIRONMENT, strategy.Environment);
    }
}
