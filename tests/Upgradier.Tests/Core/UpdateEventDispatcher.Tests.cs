using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class UpdateEventDispatcher_Tests
{

    [Fact]
    public void Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new UpdateEventDispatcher(null, null, null));
    }

    [Fact]
    public async Task NotifyBeforeBatchProcessingAsync_Notify()
    {
        IUpdateEvents events = Substitute.For<IUpdateEvents>();
        UpdateEventDispatcher dispatcher = new (events, new LogAdapter(null), null);
        await dispatcher.NotifyBeforeBatchProcessingAsync(1, null, CancellationToken.None);
        Assert.Single(events.BeforeBatchProcessingAsync.ReceivedCalls());
    }

    [Fact]
    public async Task NotifyAfterBatchProcessingAsync_Notify()
    {
        IUpdateEvents events = Substitute.For<IUpdateEvents>();
        UpdateEventDispatcher dispatcher = new(events, new LogAdapter(null), null);
        await dispatcher.NotifyAfterBatchProcessedAsync(1, null, CancellationToken.None);
        Assert.Single(events.AfterBatchProcessedAsync.ReceivedCalls());
    }
}
