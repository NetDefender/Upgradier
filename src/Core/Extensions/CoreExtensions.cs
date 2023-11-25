using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Upgradier.Core;

public static class CoreExtensions
{
    public static void ThrowIfDirectoryNotExists(this string path, string? message = null)
    {
        DirectoryInfo directoryInfo = new (path);
        if (!directoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"The path {path} doesn't exists. {message}");
        }
    }

    public static void ThrowIfIsNotAbsoluteUri(this Uri uri, string? message = null)
    {
        if (uri is null || !uri.IsAbsoluteUri || !IsHttpScheme(uri))
        {
            throw new DirectoryNotFoundException($"The value {uri} is not an absolute Uri. {message}");
        }
    }

    public static bool TryCreateUri([StringSyntax(StringSyntaxAttribute.Uri)] this string possibleUri, out Uri? uri)
    {
        if (!Uri.IsWellFormedUriString(possibleUri, UriKind.Absolute))
        {
            uri = null;
            return false;
        }
        return Uri.TryCreate(possibleUri, UriKind.Absolute, out uri);
    }

    public static bool IsHttpScheme(this Uri? uri)
    {
        return uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static UpdateBuilder WithFileBatchStrategy(this UpdateBuilder builder, string baseDirectory)
    {
        builder.WithBatchStrategy(options => new FileBatchStrategy(baseDirectory, options.Logger));
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

    public static bool IsNullOrEmptyOrWhiteSpace(this string value)
    {
        return string.IsNullOrEmpty(value) || value.IsWhiteSpace();
    }

    public static bool IsWhiteSpace(this string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static UriBuilder CombinePath(this UriBuilder builder, string path)
    {
        builder.Path = builder.Path.TrimEnd('/') + $"/{path}";
        return builder;
    }

    public static StringBuilder AppendWhen(this StringBuilder builder, Func<bool> condition, params string[] values)
    {
        if (condition())
        {
            foreach (string value in values)
            {
                builder.Append(value);
            }
        }
        return builder;
    }

    public static void ThrowIfNotExists(this FileInfo file)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException($"Could not download file to {file.FullName}", file.FullName);
        }
    }

    public static string EmptyIfNull(this string? value)
    {
        return value is null ? string.Empty : value;
    }

    public static bool CreateIfNotExists(this DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            directory.Create();
            return true;
        }
        return false;
    }
}