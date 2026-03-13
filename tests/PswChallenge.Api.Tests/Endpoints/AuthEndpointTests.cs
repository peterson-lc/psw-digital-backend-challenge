using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using PswChallenge.Application.Models.Auth;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Api.Tests.Endpoints;

public class AuthEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public AuthEndpointTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginResponse = new LoginResponseModel("test-jwt-token", DateTime.UtcNow.AddHours(1));
        _factory.AuthServiceMock
            .Setup(x => x.LoginAsync("admin@email.com", "Admin@123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        var request = new { email = "admin@email.com", password = "Admin@123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSucceededResponseWithToken()
    {
        // Arrange
        var loginResponse = new LoginResponseModel("test-jwt-token", DateTime.UtcNow.AddHours(1));
        _factory.AuthServiceMock
            .Setup(x => x.LoginAsync("admin@email.com", "Admin@123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        var request = new { email = "admin@email.com", password = "Admin@123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseModel<LoginResponseModel>>(JsonOptions);

        // Assert
        body.Should().NotBeNull();
        body!.Succeeded.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.Token.Should().Be("test-jwt-token");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        _factory.AuthServiceMock
            .Setup(x => x.LoginAsync("wrong@test.com", "WrongPass", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        var request = new { email = "wrong@test.com", password = "WrongPass" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsFailureResponseWithMessage()
    {
        // Arrange
        _factory.AuthServiceMock
            .Setup(x => x.LoginAsync("wrong@test.com", "WrongPass", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        var request = new { email = "wrong@test.com", password = "WrongPass" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseModel<LoginResponseModel>>(JsonOptions);

        // Assert
        body.Should().NotBeNull();
        body!.Succeeded.Should().BeFalse();
        body.Messages.Should().Contain("Invalid credentials.");
    }
}

