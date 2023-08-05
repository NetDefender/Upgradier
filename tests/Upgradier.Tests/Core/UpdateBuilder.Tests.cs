using NSubstitute;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class UpdateBuilder_Tests
{
    [Fact]
    public void UpdateBuilder_Throws_If_AddProviderFactories_Is_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.AddProviderFactories(null));
    }

    [Fact]
    public void UpdateBuilder_Throws_If_AddProviderFactories_Length_Is_0()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddProviderFactories(Array.Empty<Func<IProviderFactory>>()));
    }

    [Fact]
    public void UpdateBuilder_AddProviderFactories_Adds_That_Elements()
    {
        UpdateBuilder builder = new();
        Func<IProviderFactory>[] factories = new Func<IProviderFactory>[]
        {
            () => Substitute.For<IProviderFactory>(),
            () => Substitute.For<IProviderFactory>(),
            () => Substitute.For<IProviderFactory>()
        };
        builder.AddProviderFactories(factories);
        Assert.True(factories.SequenceEqual(builder.GetProviderFactories()));
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
    public void WithScriptStrategy_Throws_If_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithScriptStrategy(null));
    }

    [Fact]
    public void WithScriptStrategy_Sets_ScriptStrategy()
    {
        UpdateBuilder builder = new();
        Func<IScriptStrategy> scriptStrategy = () => Substitute.For<IScriptStrategy>();
        builder.WithScriptStrategy(scriptStrategy);
        Assert.Same(scriptStrategy, builder.GetScriptStrategy());
    }

    [Fact]
    public void WithWebScriptStrategy_Throws_ArgumentNullException_If_ScriptStrategy_Is_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithWebScriptStrategy(null, Substitute.For<Func<HttpRequestMessage, Task>>()));
    }

    [Fact]
    public void WithWebScriptStrategy_Throws_ArgumentNullException_If_ConfigureRequest_Is_Null()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentNullException>(() => builder.WithWebScriptStrategy(Substitute.For<Func<WebScriptStrategy>>(), null));
    }

    [Fact]
    public void WithWaitTimeout_Throws_ArgumentOutOfRangeException_If_Negative()
    {
        UpdateBuilder builder = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithWaitTimeout(-1));
    }

    [Fact]
    public void Build_Throws_ArgumentNullException_If_SourceProvider_Is_Not_Set()
    {
        UpdateBuilder builder = new();
        Func<IProviderFactory>[] providerFactories = new Func<IProviderFactory>[]
        {
            Substitute.For<Func<IProviderFactory>>()
        };
        builder.WithScriptStrategy(Substitute.For<Func<IScriptStrategy>>());
        builder.AddProviderFactories(providerFactories);
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }

    [Fact]
    public void Build_Throws_ArgumentNullException_If_ScriptStrategy_Is_Not_Set()
    {
        UpdateBuilder builder = new();
        Func<IProviderFactory>[] providerFactories = new Func<IProviderFactory>[]
        {
            Substitute.For<Func<IProviderFactory>>()
        };
        builder.AddProviderFactories(providerFactories);
        builder.WithSourceProvider(Substitute.For<Func<ISourceProvider>>());
        Assert.Throws<ArgumentNullException>(() => builder.Build());
    }

    [Fact]
    public void Build_Throws_ArgumentOUtOfRangeException_If_ProviderFactories_Length_Is_0()
    {
        UpdateBuilder builder = new();
        builder.WithSourceProvider(Substitute.For<Func<ISourceProvider>>());
        builder.WithScriptStrategy(Substitute.For<Func<IScriptStrategy>>());
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Build());
    }
}