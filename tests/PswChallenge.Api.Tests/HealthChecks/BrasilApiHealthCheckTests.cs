using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using PswChallenge.Api.HealthChecks;
using PswChallenge.Infra.ExternalServices.BrasilApi;
using PswChallenge.Infra.ExternalServices.BrasilApi.Models;

namespace PswChallenge.Api.Tests.HealthChecks;

public class BrasilApiHealthCheckTests
{
    private readonly Mock<IBrasilApi> _mockBrasilApi;
    private readonly Mock<ILogger<BrasilApiHealthCheck>> _mockLogger;
    private readonly BrasilApiHealthCheck _healthCheck;

    public BrasilApiHealthCheckTests()
    {
        _mockBrasilApi = new Mock<IBrasilApi>();
        _mockLogger = new Mock<ILogger<BrasilApiHealthCheck>>();
        _healthCheck = new BrasilApiHealthCheck(_mockBrasilApi.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenApiReturnsHolidays_ReturnsHealthy()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var holidays = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(currentYear, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National },
            new() { Date = new DateOnly(currentYear, 12, 25), Name = "Natal", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(currentYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Brasil API is responding normally");
        result.Data.Should().ContainKey("year").WhoseValue.Should().Be(currentYear);
        result.Data.Should().ContainKey("holidayCount").WhoseValue.Should().Be(2);
        result.Data.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenApiReturnsNull_ReturnsUnhealthy()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;

        _mockBrasilApi
            .Setup(x => x.GetHoliday(currentYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<BrasilApiHolidayResponse>)null!);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Brasil API returned null response");
        result.Data.Should().ContainKey("year").WhoseValue.Should().Be(currentYear);
        result.Data.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenTaskCanceledException_ReturnsDegraded()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var exception = new TaskCanceledException("Request timeout");

        _mockBrasilApi
            .Setup(x => x.GetHoliday(currentYear, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("Brasil API health check timed out");
        result.Exception.Should().Be(exception);
        result.Data.Should().ContainKey("timeout").WhoseValue.Should().Be("5 seconds");
        result.Data.Should().ContainKey("timestamp");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHttpRequestException_ReturnsUnhealthy()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var exception = new HttpRequestException("503 Service Unavailable");

        _mockBrasilApi
            .Setup(x => x.GetHoliday(currentYear, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Brasil API is not reachable");
        result.Exception.Should().Be(exception);
        result.Data.Should().ContainKey("error").WhoseValue.Should().Be("503 Service Unavailable");
        result.Data.Should().ContainKey("timestamp");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenGenericException_ReturnsUnhealthy()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var exception = new InvalidOperationException("Unexpected failure");

        _mockBrasilApi
            .Setup(x => x.GetHoliday(currentYear, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Brasil API health check failed");
        result.Exception.Should().Be(exception);
        result.Data.Should().ContainKey("error").WhoseValue.Should().Be("Unexpected failure");
        result.Data.Should().ContainKey("timestamp");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

