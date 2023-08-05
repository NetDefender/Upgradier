using Upgradier.Core;

namespace Upgradier.Tests.Core
{
    public sealed class DatabaseVersion_Tests
    {
        [Fact]
        public void VersionId_Is_Settable_And_Accesible()
        {
            DatabaseVersion version = new()
            { VersionId = 1 };
            Assert.Equal(1, version.VersionId);
        }

        [Fact]
        public void VersionId_Can_Be_Negative()
        {
            DatabaseVersion version = new()
            { VersionId = -1 };
            Assert.Equal(-1, version.VersionId);
        }
    }
}
