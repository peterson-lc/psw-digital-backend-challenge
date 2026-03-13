using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Models.Auth;

[ExcludeFromCodeCoverage]
public record LoginRequestModel(string Email, string Password);