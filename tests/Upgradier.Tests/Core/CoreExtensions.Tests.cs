using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upgradier.Core;

namespace Upgradier.Tests.Core;

public sealed class CoreExtensions_Tests
{
    [Fact]
    public void ThrowIfDirectoryNotExists_Throws_With_Message()
    {
        const string MESSAGE = "_Message_";
        const string directory = "_NonExistentDirectory_";
        DirectoryNotFoundException exception = Assert.Throws<DirectoryNotFoundException>(() => directory.ThrowIfDirectoryNotExists(MESSAGE));
        Assert.Equal($"The path {directory} doesn't exists. {MESSAGE}", exception.Message);
    }

    [Fact]
    public void ThrowIfIsNotAbsoluteUri_Throws_If_Is_Null()
    {
        Uri? uri = null;
        Assert.Throws<DirectoryNotFoundException>(() => uri.ThrowIfIsNotAbsoluteUri());
    }

    [Fact]
    public void ThrowIfIsNotAbsoluteUri_Throws_If_Is_Not_Absolute_Uri()
    {
        Uri uri = new ("https://www.point.es/get");
        Uri relativeUri = uri.MakeRelativeUri(new Uri("https://www.point.es"));
        Assert.Throws<DirectoryNotFoundException>(() => relativeUri.ThrowIfIsNotAbsoluteUri());
    }

    [Fact]
    public void ThrowIfIsNotAbsoluteUri_Throws_If_Is_Not_Http_Scheme()
    {
        Uri uri = new ("ftp://www.point.es/get");
        Assert.Throws<DirectoryNotFoundException>(() => uri.ThrowIfIsNotAbsoluteUri());
    }

    [Theory]
    [InlineData("http")]
    [InlineData("https")]
    public void ThrowIfIsNotAbsoluteUri_Works_If_Is_Http_Scheme(string scheme)
    {
        Uri uri = new ($"{scheme}://www.point.es/get");
        uri.ThrowIfIsNotAbsoluteUri();
    }

    [Fact]
    public void TryCreateUri_Returns_False_If_Not_IsWellFormedUriString()
    {
        bool created = "www.point.es/get".TryCreateUri(out Uri? uri);
        Assert.False(created);
    }

    [Fact]
    public void TryCreateUri_Outs_Null_Uri_If_Not_IsWellFormedUriString()
    {
        "www.point.es/get".TryCreateUri(out Uri? uri);
        Assert.Null(uri);
    }
}
