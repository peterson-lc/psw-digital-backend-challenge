using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PswChallenge.Api.Tests.HealthChecks;

public class HealthCheckEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        healthReport.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthCheck_IncludesTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        healthReport.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        timestamp.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task HealthCheck_IncludesDuration()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        healthReport.TryGetProperty("duration", out var duration).Should().BeTrue();
        duration.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthCheck_IncludesChecksArray()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        healthReport.TryGetProperty("checks", out var checks).Should().BeTrue();
        checks.ValueKind.Should().Be(JsonValueKind.Array);
        checks.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HealthCheck_IncludesSelfCheck()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        var checks = healthReport.GetProperty("checks");
        var selfCheck = checks.EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("name").GetString() == "self");

        selfCheck.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        selfCheck.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthCheck_DoesNotRequireAuthentication()
    {
        // Arrange - No authentication header

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Healthz_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Healthz_DoesNotRequireAuthentication()
    {
        // Arrange - No authentication header

        // Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}

