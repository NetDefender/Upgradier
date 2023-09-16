using System;
using System.Diagnostics.CodeAnalysis;
using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class UpdateBuilder_Tests
{
    [Fact]
    public void UpdateBuilder_Throws_If_AddProviderFactories_Is_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.AddDatabaseEngines(null));
    }

    [Fact]
    public void UpdateBuilder_Throws_If_AddProviderFactories_Length_Is_0()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddDatabaseEngines(Array.Empty<Func<IDatabaseEngine>>()));
    }

    [Fact]
    public void UpdateBuilder_AddProviderFactories_Adds_That_Elements()
    {
        UpdateBuilder builder = new();
        Func<IDatabaseEngine>[] factories = new Func<IDatabaseEngine>[]
        {
            () => Substitute.For<IDatabaseEngine>(),
            () => Substitute.For<IDatabaseEngine>(),
            () => Substitute.For<IDatabaseEngine>()
        };
        builder.AddDatabaseEngines(factories);
        Assert.True(factories.SequenceEqual(builder.GetDatabaseEngines()));
    }

    [Fact]
    public void WithSourceProvider_Throws_If_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithSourceProvider(null));
    }

    [Fact]
    public void WithSourceProvider_Sets_SourceProvider()
    {
        UpdateBuilder builder = new();
        Func<ISourceProvider> sourceProvider = () => Substitute.For<ISourceProvider>();
        builder.WithSourceProvider(sourceProvider);
        Assert.Same(sourceProvider, builder.GetSourceProvider());
    }

    [Fact]
    public void WithBatchStrategy_Throws_If_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithBatchStrategy(null));
    }

    [Fact]
    public void WithBatchStrategy_Sets_BatchStrategy()
    {
        UpdateBuilder builder = new();
        Func<IBatchStrategy> batchStrategy = () => Substitute.For<IBatchStrategy>();
        builder.WithBatchStrategy(batchStrategy);
        Assert.Same(batchStrategy, builder.GetBatchStrategy());
    }

    [Fact]
    public void WithWebBatchStrategy_Throws_ArgumentNullException_If_BatchStrategy_Is_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithWebBatchStrategy(null, Substitute.For<Func<HttpRequestMessage, Task>>()));
    }

    [Fact]
    public void WithWebBatchStrategy_Throws_ArgumentNullException_If_ConfigureRequest_Is_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithWebBatchStrategy(Substitute.For<Func<WebBatchStrategy>>(), null));
    }

    [Fact]
    public void WithWaitTimeout_Throws_ArgumentOutOfRangeException_If_Negative()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithWaitTimeout(-1));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(1000)]
    public void WithWaitTimeout_SetsTimeout(int timeout)
    {
        UpdateBuilder builder = new();
        builder.WithWaitTimeout(timeout);
        Assert.Equal(timeout, builder.GetWaitTimeout());
    }

    [Fact]
    public void Build_Throws_ArgumentNullException_If_SourceProvider_Is_Not_Set()
    {
        UpdateBuilder builder = new();
        Func<IDatabaseEngine>[] providerFactories = new Func<IDatabaseEngine>[]
        {
            Substitute.For<Func<IDatabaseEngine>>()
        };
        builder.WithBatchStrategy(Substitute.For<Func<IBatchStrategy>>());
        builder.AddDatabaseEngines(providerFactories);
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }

    [Fact]
    public void Build_Throws_ArgumentNullException_If_BatchStrategy_Is_Not_Set()
    {
        UpdateBuilder builder = new();
        Func<IDatabaseEngine>[] providerFactories = new Func<IDatabaseEngine>[]
        {
            Substitute.For<Func<IDatabaseEngine>>()
        };
        builder.AddDatabaseEngines(providerFactories);
        builder.WithSourceProvider(Substitute.For<Func<ISourceProvider>>());
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }

    [Fact]
    public void Build_Throws_ArgumentOUtOfRangeException_If_ProviderFactories_Length_Is_0()
    {
        UpdateBuilder builder = new();
        builder.WithSourceProvider(Substitute.For<Func<ISourceProvider>>());
        builder.WithBatchStrategy(Substitute.For<Func<IBatchStrategy>>());
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Build());
    }

    [Theory]
    [InlineData("https://wwww.page.es", "SqlServer", "Dev")]
    public void WithWebBatchStrategy_Sets_BatchStrategy([StringSyntax(StringSyntaxAttribute.Uri)]string uri, string provider, string? environment)
    {
        WebBatchStrategy webStrategy = Substitute.For<WebBatchStrategy>(new Uri(uri), environment);
        Func<WebBatchStrategy> webStrategyBuilder = () => webStrategy;
        UpdateBuilder builder = new();
        Func<HttpRequestMessage, Task> httpMessageOptions = (message) => Task.CompletedTask;
        builder.WithWebBatchStrategy(webStrategyBuilder, httpMessageOptions);
        Assert.Same(webStrategy, builder.GetBatchStrategy()?.Invoke());
    }

    [Fact]
    public void Build_Returns_UpdateManager()
    {
        UpdateBuilder builder = new();
        IDatabaseEngine factory = Substitute.For<IDatabaseEngine>();
        factory.Name.Returns("ProviderA");
        Func<IDatabaseEngine>[] factories = new Func<IDatabaseEngine>[] { () => factory };
        builder.AddDatabaseEngines(factories);
        WebBatchStrategy webStrategy = Substitute.For<WebBatchStrategy>(new Uri("http://invent.com"), "Dev");
        Func<WebBatchStrategy> webStrategyFactory = () => webStrategy;
        Func<HttpRequestMessage, Task> httpMessageOptions = (message) => Task.CompletedTask;
        builder.WithWebBatchStrategy(webStrategyFactory, httpMessageOptions);
        builder.WithSourceProvider(() => Substitute.For<ISourceProvider>());
        builder.WithWaitTimeout(100);
        UpdateManager updateManager = builder.Build();
        Assert.NotNull(updateManager);
    }
}