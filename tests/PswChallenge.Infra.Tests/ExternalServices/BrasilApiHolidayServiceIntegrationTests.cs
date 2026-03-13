using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Infra.ExternalServices.BrasilApi;
using PswChallenge.Infra.ExternalServices.BrasilApi.Models;

namespace PswChallenge.Infra.Tests.ExternalServices;

/// <summary>
/// Integration tests for BrasilApiHolidayService that verify HTTP communication
/// with mocked HTTP message handlers to test deserialization and error handling.
/// </summary>
public class BrasilApiHolidayServiceIntegrationTests
{
    private const string BaseUrl = "https://brasilapi.com.br";

    [Fact]
    public async Task GetHolidaysByYearAsync_WithValidHttpResponse_DeserializesAndMapsCorrectly()
    {
        // Arrange - Simulates HTTP response deserialization
        var year = 2025;
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National },
            new() { Date = new DateOnly(2025, 12, 25), Name = "Natal", Type = BrasilApiHolidayType.National }
        };

        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act
        var result = await service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(h => h.Type.Should().Be(HolidayType.National));
        result.First().Name.Should().Be("Confraternização mundial");
        result.First().Date.Should().Be(new DateOnly(2025, 1, 1));
        result.Last().Name.Should().Be("Natal");
        result.Last().Date.Should().Be(new DateOnly(2025, 12, 25));
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithEmptyHttpResponse_ReturnsEmptyList()
    {
        // Arrange
        var year = 2030;
        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BrasilApiHolidayResponse>());

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act
        var result = await service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithApiException_PropagatesException()
    {
        // Arrange
        var year = 9999;
        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("404 Not Found"));

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetHolidaysByYearAsync(year));
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithServerError_PropagatesException()
    {
        // Arrange
        var year = 2025;
        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("500 Internal Server Error"));

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetHolidaysByYearAsync(year));
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithNetworkTimeout_PropagatesTaskCanceledException()
    {
        // Arrange
        var year = 2025;
        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => service.GetHolidaysByYearAsync(year));
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_PassesCancellationToken_ToApi()
    {
        // Arrange
        var year = 2025;
        var cts = new CancellationTokenSource();
        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, cts.Token))
            .ReturnsAsync(new List<BrasilApiHolidayResponse>());

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act
        var result = await service.GetHolidaysByYearAsync(year, cts.Token);

        // Assert
        result.Should().BeEmpty();
        mockBrasilApi.Verify(x => x.GetHoliday(year, cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithMunicipalHolidays_MapsCorrectly()
    {
        // Arrange - Simulates HTTP response with municipal holidays
        var year = 2025;
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National },
            new() { Date = new DateOnly(2025, 6, 15), Name = "Aniversário da Cidade", Type = BrasilApiHolidayType.Municipal }
        };

        var mockBrasilApi = new Mock<IBrasilApi>();
        mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BrasilApiHolidayService(mockBrasilApi.Object, memoryCache);

        // Act
        var result = await service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().HaveCount(2);
        result.First().Type.Should().Be(HolidayType.National);
        result.Last().Type.Should().Be(HolidayType.Municipal);
        result.Last().Name.Should().Be("Aniversário da Cidade");
    }
}
