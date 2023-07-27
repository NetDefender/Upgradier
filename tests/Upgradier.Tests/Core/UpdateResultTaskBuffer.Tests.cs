using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public class UpdateResultTaskBuffer_Tests
{
    [Fact]
    public void Throws_When_Logger_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new UpdateResultTaskBuffer(1, null, null));
    }

    [Fact]
    public void Throws_When_Parallelism_Below_1()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UpdateResultTaskBuffer(0, new LogAdapter(null), null));
    }

    [Fact]
    public void Throws_When_Parallelism_Above_ProcessorCount_x_20()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UpdateResultTaskBuffer(1000, new LogAdapter(null), null));
    }

    [Fact]
    public void Try_Add_Returns_True_When_Buffer_Is_Empty()
    {
        UpdateResultTaskBuffer buffer = new (1, new LogAdapter(null), null);
        Assert.True(buffer.TryAdd(Task.FromResult(new UpdateResult("", string.Empty, string.Empty, 0, 0, null, null))));
    }

    [Fact]
    public void Try_Add_Returns_False_When_Buffer_Is_Full()
    {
        UpdateResultTaskBuffer buffer = new (1, new LogAdapter(null), null);
        Assert.True(buffer.TryAdd(Task.FromResult(new UpdateResult("", string.Empty, string.Empty, 0, 0, null, null))));
        Assert.False(buffer.TryAdd(Task.FromResult(new UpdateResult("", string.Empty, string.Empty, 0, 0, null, null))));
    }

    [Fact]
    public async Task WhenAll_Returns_Two_Task_Results()
    {
        UpdateResultTaskBuffer buffer = new (2, new LogAdapter(null), null);
        buffer.TryAdd(Task.FromResult(new UpdateResult("1", string.Empty, string.Empty, 0, 0, null, null)));
        buffer.TryAdd(Task.FromResult(new UpdateResult("2", string.Empty, string.Empty, 0, 0, null, null)));
        UpdateResult[] results = await buffer.WhenAll();
        Assert.Equal("1", results[0].Source);
        Assert.Equal("2", results[1].Source);
    }

    [Fact]
    public async Task Clear_Deletes_The_Buffer()
    {
        UpdateResultTaskBuffer buffer = new (2, new LogAdapter(null), null);
        buffer.TryAdd(Task.FromResult(new UpdateResult("1", string.Empty, string.Empty, 0, 0, null, null)));
        buffer.TryAdd(Task.FromResult(new UpdateResult("2", string.Empty, string.Empty, 0, 0, null, null)));
        buffer.Clear();
        UpdateResult[] results = await buffer.WhenAll();
        Assert.Empty(results);
    }
}
