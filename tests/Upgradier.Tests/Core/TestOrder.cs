namespace Upgradier.Tests.Core;

public sealed class TestOrderAttribute : Attribute
{
    public TestOrderAttribute(int order)
    {
        Order = order;
    }

    public int Order { get; }
}
