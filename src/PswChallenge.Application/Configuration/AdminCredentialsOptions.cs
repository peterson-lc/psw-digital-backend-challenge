using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Configuration;

[ExcludeFromCodeCoverage]
public sealed class AdminCredentialsOptions
{
    public const string SectionName = "AdminCredentials";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

