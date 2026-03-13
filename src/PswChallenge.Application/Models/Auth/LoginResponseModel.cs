using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Models.Auth;

[ExcludeFromCodeCoverage]
public record LoginResponseModel(string Token, DateTime Expiration);