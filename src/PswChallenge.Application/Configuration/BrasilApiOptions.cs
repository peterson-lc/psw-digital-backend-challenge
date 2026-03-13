using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Configuration;

[ExcludeFromCodeCoverage]
public sealed class BrasilApiOptions
{
    public const string SectionName = "ExternalApis:BrasilApi";

    public string BaseUrl { get; init; } = string.Empty;
}

