using System.Linq.Expressions;
using System.Text;
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

    [Theory]
    [InlineData("http://www.google.es")]
    [InlineData("https://www.google.es")]
    public void IsHttpScheme_Returns_True_If_Http_Https(string uri)
    {
        Assert.True(new Uri(uri).IsHttpScheme());
    }

    [Theory]
    [InlineData("ftp://www.google.es")]
    [InlineData("ssh://www.google.es")]
    public void IsHttpScheme_Returns_False_If_Not_Http_Https(string uri)
    {
        Assert.False(new Uri(uri).IsHttpScheme());
    }

    [Theory]
    [InlineData("1.sql")]
    [InlineData("999.sql")]
    public void ResourceId_Returns_FirstPart(string resource)
    {
        int id = int.Parse(resource.Split('.')[0]);
        Assert.Equal(id, resource.ResourceId());
    }

    [Theory]
    [InlineData("A   ")]
    [InlineData("B        ")]
    public void TrimEnd_Deletes_Spaces_In_End(string text)
    {
        string expected = text.TrimEnd();
        string actual = new StringBuilder(text).TrimEnd(' ').ToString();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("BCA")]
    public void RemoveLast_Deletes_Last_Character(string text)
    {
        string expected = text[..^1];
        string actual = new StringBuilder(text).RemoveLast().ToString();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void IsNullOrEmptyOrWhiteSpace_Remove_When_Null_Empty_WhiteSpace(string text)
    {
        string expected = text[..^1];
        string actual = new StringBuilder(text).RemoveLast().ToString();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("    ")]
    [InlineData("         ")]
    public void IsWhiteSpace_Returns_True_When_IsWhiteSpace(string text)
    {
        Assert.True(text.IsWhiteSpace());
    }

    [Theory]
    [InlineData("A    ")]
    [InlineData(" B        ")]
    public void IsWhiteSpace_Returns_False_When_Any_Character_IsNot_WhiteSpace(string text)
    {
        Assert.False(text.IsWhiteSpace());
    }

    [Theory]
    [InlineData("http://www.google.es/", "FirstPath")]
    [InlineData("http://www.google.es", "SecondPath")]
    public void CombinePath_Trims_End_Backslash_Before_Combine(string uri, string path)
    {
        UriBuilder builder = new (uri);
        string builderPath = builder.Path;
        string expectedPath = builderPath.TrimEnd('/') + "/" + path;
        string actualPath = builder.CombinePath(path).Path;
        Assert.Equal(expectedPath, actualPath);
    }

    [Fact]
    public void AppendWhen_Appends_When_Condition_IsTrue()
    {
        StringBuilder builder = new ();
        builder.AppendWhen(() => true, "Hello");
        Assert.Equal("Hello", builder.ToString());
    }

    [Fact]
    public void AppendWhen_Not_Appends_When_Condition_IsFalse()
    {
        StringBuilder builder = new();
        builder.AppendWhen(() => false, "Hello");
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void ThrowIfNotExists_Throws_When_File_Doesnt_Exists()
    {
        FileInfo file = new ($"c://Path/doesnt/exists/{Guid.NewGuid()}.txt");
        Assert.Throws<FileNotFoundException>(() => file.ThrowIfNotExists());
    }

    [Fact]
    public void EmptyIfNull_Returns_Empty_When_Null()
    {
        string expected = string.Empty;
        string actual = ((string)null).EmptyIfNull();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateIfNotExists_CreateDirectory_When_NotExists()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(),$"{Guid.NewGuid()}");
        DirectoryInfo directory = new (directoryPath);
        Assert.False(directory.Exists);
        Assert.True(directory.CreateIfNotExists());
        Assert.True(directory.Exists);
    }
}
