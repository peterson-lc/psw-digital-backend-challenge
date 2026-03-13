using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PswChallenge.Application.Configuration;
using PswChallenge.Application.Services;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IOptions<JwtOptions>> _mockJwtOptions;
    private readonly Mock<IOptions<AdminCredentialsOptions>> _mockAdminOptions;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        _mockAdminOptions = new Mock<IOptions<AdminCredentialsOptions>>();

        var jwtOptions = new JwtOptions
        {
            SecretKey = "this-is-a-very-long-secret-key-for-testing-purposes-only-1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        };

        var adminOptions = new AdminCredentialsOptions
        {
            Username = "admin@test.com",
            Password = "AdminPassword123!"
        };

        _mockJwtOptions.Setup(x => x.Value).Returns(jwtOptions);
        _mockAdminOptions.Setup(x => x.Value).Returns(adminOptions);

        _authService = new AuthService(_mockAdminOptions.Object, _mockJwtOptions.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndExpiration()
    {
        // Arrange
        var email = "admin@test.com";
        var password = "AdminPassword123!";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var email = "wrong@test.com";
        var password = "AdminPassword123!";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(email, password));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var email = "admin@test.com";
        var password = "WrongPassword";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(email, password));
    }

    [Fact]
    public async Task LoginAsync_GeneratedToken_ContainsCorrectClaims()
    {
        // Arrange
        var email = "admin@test.com";
        var password = "AdminPassword123!";

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
        // Token should be a valid JWT (contains 3 parts separated by dots)
        result.Token.Split('.').Should().HaveCount(3);
    }
}

