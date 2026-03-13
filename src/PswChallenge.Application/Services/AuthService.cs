using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PswChallenge.Application.Configuration;
using PswChallenge.Application.Models.Auth;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Application.Services;

public sealed class AuthService(
    IOptions<AdminCredentialsOptions> adminCredentials,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly AdminCredentialsOptions _adminCredentials = adminCredentials.Value;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public Task<LoginResponseModel> LoginAsync(string username, string password)
    {
        if (!string.Equals(username, _adminCredentials.Username, StringComparison.Ordinal) ||
            !string.Equals(password, _adminCredentials.Password, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = GenerateJwtToken(username);
        return Task.FromResult(token);
    }

    private LoginResponseModel GenerateJwtToken(string username)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new LoginResponseModel(tokenString, expiration);
    }
}

