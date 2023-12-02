using System.Diagnostics.CodeAnalysis;

namespace Upgradier.Core;

[ExcludeFromCodeCoverage]
public sealed class SourceProviderCreationOptions : BaseCreationOptions
{
    public IEncryptor? Encryptor { get; init; }
}