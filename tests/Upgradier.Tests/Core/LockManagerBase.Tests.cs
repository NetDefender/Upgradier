using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class LockManagerBaseTests
{
    [Fact]
    public void Ctor_Throws_If_SourceDatabase_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            try
            {
                Substitute.For<LockManagerBase>(null, new LogAdapter(null));
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        });
    }
}
