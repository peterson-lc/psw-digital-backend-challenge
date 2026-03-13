namespace PswChallenge.Application.Configuration;

public sealed class AdminCredentialsOptions
{
    public const string SectionName = "AdminCredentials";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

