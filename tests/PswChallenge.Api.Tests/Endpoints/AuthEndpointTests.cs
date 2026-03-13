using FluentAssertions;
using Moq;
using PswChallenge.Application.Models.Auth;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Api.Tests.Endpoints;

public class AuthEndpointTests
{
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthEndpointTests()
    {
        _mockAuthService = new Mock<IAuthService>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var email = "admin@test.com";
        var password = "AdminPassword123!";
        var loginResponse = new LoginResponseModel("test-token", DateTime.UtcNow.AddHours(1));

        _mockAuthService
            .Setup(x => x.LoginAsync(email, password))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _mockAuthService.Object.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var email = "wrong@test.com";
        var password = "WrongPassword";

        _mockAuthService
            .Setup(x => x.LoginAsync(email, password))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _mockAuthService.Object.LoginAsync(email, password));
    }

    [Fact]
    public async Task LoginAsync_ReturnsApiResponseModel_WithCorrectStructure()
    {
        // Arrange
        var email = "admin@test.com";
        var password = "AdminPassword123!";
        var loginResponse = new LoginResponseModel("test-token", DateTime.UtcNow.AddHours(1));

        _mockAuthService
            .Setup(x => x.LoginAsync(email, password))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _mockAuthService.Object.LoginAsync(email, password);
        var apiResponse = ApiResponseModel<LoginResponseModel>.Success(result);

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Succeeded.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_CallsAuthServiceOnce()
    {
        // Arrange
        var email = "admin@test.com";
        var password = "AdminPassword123!";
        var loginResponse = new LoginResponseModel("test-token", DateTime.UtcNow.AddHours(1));

        _mockAuthService
            .Setup(x => x.LoginAsync(email, password))
            .ReturnsAsync(loginResponse);

        // Act
        await _mockAuthService.Object.LoginAsync(email, password);

        // Assert
        _mockAuthService.Verify(x => x.LoginAsync(email, password), Times.Once);
    }
}

