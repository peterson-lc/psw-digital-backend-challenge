namespace PswChallenge.Application.Models.Auth;

public record LoginResponseModel(string Token, DateTime Expiration);