using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PswChallenge.Api.Tests.Cors;

public class CorsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task PreflightRequest_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").First().Should().Be("*");
    }

    [Fact]
    public async Task PreflightRequest_ReturnsAccessControlAllowMethodsHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Methods");
    }

    [Fact]
    public async Task PreflightRequest_ReturnsAccessControlAllowHeadersHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Headers");
    }

    [Fact]
    public async Task GetRequest_WithOriginHeader_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "https://example.com");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").First().Should().Be("*");
    }

    [Fact]
    public async Task PostRequest_WithOriginHeader_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
        request.Headers.Add("Origin", "https://example.com");
        request.Content = new StringContent("{\"username\":\"admin\",\"password\":\"admin123\"}", 
            System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").First().Should().Be("*");
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://app.example.org")]
    public async Task Request_WithDifferentOrigins_AllowsAllOrigins(string origin)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").First().Should().Be("*");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task PreflightRequest_WithDifferentMethods_AllowsAllMethods(string method)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", method);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Methods");
    }

    [Theory]
    [InlineData("Authorization")]
    [InlineData("Content-Type")]
    [InlineData("X-Custom-Header")]
    public async Task PreflightRequest_WithDifferentHeaders_AllowsAllHeaders(string header)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", header);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Headers");
    }
}

