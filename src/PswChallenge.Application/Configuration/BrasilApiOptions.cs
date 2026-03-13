namespace PswChallenge.Application.Configuration;

public sealed class BrasilApiOptions
{
    public const string SectionName = "ExternalApis:BrasilApi";

    public string BaseUrl { get; init; } = string.Empty;
}

