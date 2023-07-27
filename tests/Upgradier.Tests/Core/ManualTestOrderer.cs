using Xunit.Abstractions;
using Xunit.Sdk;

namespace Upgradier.Tests.Core;

public sealed class ManualTestOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
    {
        List<(int Order, TTestCase Test)> orders = new();
        foreach (TTestCase testCase in testCases)
        {
            IEnumerable<IAttributeInfo> attributes = testCase.TestMethod.Method.GetCustomAttributes(typeof(TestOrderAttribute));
            IAttributeInfo? orderAttribute = attributes.FirstOrDefault();
            if (orderAttribute is not null)
            {
                orders.Add((Order: (int)orderAttribute.GetConstructorArguments().First(), Test: testCase));
            }
        }

        orders.Sort(((int Order, TTestCase Test) x, (int Order, TTestCase Test) y) =>
        {
            return x.Order.CompareTo(y.Order);
        });

        return orders.Select(o => o.Test);
    }
}
