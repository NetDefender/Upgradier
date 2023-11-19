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
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddDatabaseEngines(Array.Empty<IDatabaseEngine>()));
    }

    [Fact]
    public void WithSourceProvider_Throws_If_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithSourceProvider(null));
    }

    [Fact]
    public void WithBatchStrategy_Throws_If_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithBatchStrategy(null));
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
        Assert.Throws<ArgumentNullException>(() => builder.WithWebBatchStrategy(Substitute.For<WebBatchStrategy>(), null));
    }

    [Fact]
    public void Build_Throws_ArgumentNullException_If_SourceProvider_Is_Not_Set()
    {
        UpdateBuilder builder = new();
        IDatabaseEngine[] providerFactories = new IDatabaseEngine[]
        {
            Substitute.For<IDatabaseEngine>()
        };
        builder.WithBatchStrategy(Substitute.For<IBatchStrategy>());
        builder.AddDatabaseEngines(providerFactories);
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }

    [Fact]
    public void Build_Throws_ArgumentNullException_If_BatchStrategy_Is_Not_Set()
    {
        UpdateBuilder builder = new();
        IDatabaseEngine[] providerFactories = new IDatabaseEngine[]
        {
            Substitute.For<IDatabaseEngine>()
        };
        builder.AddDatabaseEngines(providerFactories);
        builder.WithSourceProvider(Substitute.For<ISourceProvider>());
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }

    [Fact]
    public void Build_Throws_ArgumentOUtOfRangeException_If_ProviderFactories_Length_Is_0()
    {
        UpdateBuilder builder = new();
        builder.WithSourceProvider(Substitute.For<ISourceProvider>());
        builder.WithBatchStrategy(Substitute.For<IBatchStrategy>());
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Build());
    }

    [Fact]
    public void Build_Returns_UpdateManager()
    {
        UpdateBuilder builder = new();
        IDatabaseEngine factory = Substitute.For<IDatabaseEngine>();
        factory.Name.Returns("ProviderA");
        builder.AddDatabaseEngines([factory]);
        WebBatchStrategy webStrategy = Substitute.For<WebBatchStrategy>(new Uri("http://invent.com"));
        Func<HttpRequestMessage, Task> httpMessageOptions = (message) => Task.CompletedTask;
        builder.WithWebBatchStrategy(webStrategy, httpMessageOptions);
        builder.WithSourceProvider(Substitute.For<ISourceProvider>());
        UpdateManager updateManager = builder.Build();
        Assert.NotNull(updateManager);
    }
}