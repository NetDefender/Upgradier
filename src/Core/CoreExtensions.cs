using System.Diagnostics.CodeAnalysis;
using System.Text;

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
        if (!IsAbsoluteUriHttp(possibleUri))
        {
            throw new DirectoryNotFoundException($"The value {possibleUri} is not an absolute Uri. {message}");
        }
    }

    public static bool IsAbsoluteUriHttp([StringSyntax(StringSyntaxAttribute.Uri)] this string possibleUri)
    {
        return Uri.TryCreate(possibleUri, UriKind.Absolute, out Uri? outUri)
            && outUri is not null && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }

    public static UpdateBuilder WithFileScriptAdapter(this UpdateBuilder builder, string baseDirectory, string provider, string? environment = null)
    {
        builder.WithScriptAdapter(() => new FileScriptStrategy(baseDirectory, provider, environment));
        return builder;
    }

    public static int ResourceId(this string resource)
    {
        string[] parts = resource.Split('.');
        return int.Parse(parts[^2]);
    }

    public static StringBuilder TrimEnd(this StringBuilder builder, params char[] trims)
    {
        if (trims?.Length > 0)
        {
            while (builder.Length > 0 && Array.Exists(trims, trim => trim == builder[^1]))
            {
                builder.RemoveLast();
            }
        }
        return builder;
    }

    public static StringBuilder RemoveLast(this StringBuilder builder)
    {
        builder.Remove(builder.Length - 1, 1);
        return builder;
    }
}