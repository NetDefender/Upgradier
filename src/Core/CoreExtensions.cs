using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core;

public static class CoreExtensions
{
    public static void ThrowIfDirectoryNotExists(string path, string? message = null)
    {
        DirectoryInfo directoryInfo = new (path);
        if (!directoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"The path {path} doesn't exists. {message}");
        }
    }

    public static void ThrowIfStringIsNotAbsoluteWebResource([StringSyntax(StringSyntaxAttribute.Uri)]string possibleUri, string? message = null)
    {
        if (!IsAbsoluteUri(possibleUri))
        {
            throw new DirectoryNotFoundException($"The value {possibleUri} is not an absolute Uri. {message}");
        }
    }

    public static bool IsAbsoluteUri([StringSyntax(StringSyntaxAttribute.Uri)] this string possibleUri)
    {
        return Uri.TryCreate(possibleUri, UriKind.Absolute, out Uri? outUri)
            && outUri is not null && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }

    public static UpdateBuilder WithFileScriptAdapter(this UpdateBuilder builder, string baseDirectory, string provider, string? environment = null)
    {
        builder.WithScriptAdapter(() => new FileScriptAdapter(baseDirectory, provider, environment));
        return builder;
    }

    public static int ResourceId(this string resource)
    {
        string[] parts = resource.Split('.');
        return int.Parse(parts[^2]);
    }
}