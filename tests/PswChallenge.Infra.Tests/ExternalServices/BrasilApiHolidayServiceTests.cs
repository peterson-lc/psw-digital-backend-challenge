using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Infra.ExternalServices.BrasilApi;
using PswChallenge.Infra.ExternalServices.BrasilApi.Models;

namespace PswChallenge.Infra.Tests.ExternalServices;

public class BrasilApiHolidayServiceTests
{
    private readonly Mock<IBrasilApi> _mockBrasilApi;
    private readonly IMemoryCache _memoryCache;
    private readonly BrasilApiHolidayService _service;

    public BrasilApiHolidayServiceTests()
    {
        _mockBrasilApi = new Mock<IBrasilApi>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new BrasilApiHolidayService(_mockBrasilApi.Object, _memoryCache);
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithValidYear_ReturnsMappedHolidays()
    {
        // Arrange
        var year = 2025;
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National },
            new() { Date = new DateOnly(2025, 12, 25), Name = "Natal", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(h => h.Type.Should().Be(HolidayType.National));
        result.First().Name.Should().Be("Confraternização mundial");
        result.Last().Name.Should().Be("Natal");
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var year = 2030;
        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BrasilApiHolidayResponse>());

        // Act
        var result = await _service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_MapsDateCorrectly()
    {
        // Arrange
        var year = 2025;
        var expectedDate = new DateOnly(2025, 4, 21);
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = expectedDate, Name = "Tiradentes", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.GetHolidaysByYearAsync(year);

        // Assert
        result.First().Date.Should().Be(expectedDate);
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_PassesCancellationTokenToApi()
    {
        // Arrange
        var year = 2025;
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, token))
            .ReturnsAsync(new List<BrasilApiHolidayResponse>());

        // Act
        await _service.GetHolidaysByYearAsync(year, token);

        // Assert
        _mockBrasilApi.Verify(x => x.GetHoliday(year, token), Times.Once);
    }


    [Fact]
    public async Task GetHolidaysByYearAsync_OnFirstCall_CallsApiAndCachesResult()
    {
        // Arrange
        var year = 2025;
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().HaveCount(1);
        _mockBrasilApi.Verify(x => x.GetHoliday(year, It.IsAny<CancellationToken>()), Times.Once);

        // Verify data is cached
        var cacheKey = $"BrasilApi_Holidays_{year}";
        _memoryCache.TryGetValue(cacheKey, out IEnumerable<HolidayDto>? cachedData).Should().BeTrue();
        cachedData.Should().NotBeNull();
        cachedData.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_OnSubsequentCalls_ReturnsCachedDataWithoutCallingApi()
    {
        // Arrange
        var year = 2025;
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National },
            new() { Date = new DateOnly(2025, 12, 25), Name = "Natal", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act - First call
        var firstResult = await _service.GetHolidaysByYearAsync(year);

        // Act - Second call (should use cache)
        var secondResult = await _service.GetHolidaysByYearAsync(year);

        // Act - Third call (should use cache)
        var thirdResult = await _service.GetHolidaysByYearAsync(year);

        // Assert
        firstResult.Should().HaveCount(2);
        secondResult.Should().HaveCount(2);
        thirdResult.Should().HaveCount(2);

        // API should only be called once
        _mockBrasilApi.Verify(x => x.GetHoliday(year, It.IsAny<CancellationToken>()), Times.Once);

        // All results should be the same
        secondResult.Should().BeEquivalentTo(firstResult);
        thirdResult.Should().BeEquivalentTo(firstResult);
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithDifferentYears_CachesSeparately()
    {
        // Arrange
        var year2025 = 2025;
        var year2026 = 2026;

        var apiResponse2025 = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National }
        };

        var apiResponse2026 = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2026, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National },
            new() { Date = new DateOnly(2026, 12, 25), Name = "Natal", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year2025, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse2025);

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse2026);

        // Act
        var result2025 = await _service.GetHolidaysByYearAsync(year2025);
        var result2026 = await _service.GetHolidaysByYearAsync(year2026);

        // Call again to verify cache
        var result2025Again = await _service.GetHolidaysByYearAsync(year2025);
        var result2026Again = await _service.GetHolidaysByYearAsync(year2026);

        // Assert
        result2025.Should().HaveCount(1);
        result2026.Should().HaveCount(2);
        result2025Again.Should().HaveCount(1);
        result2026Again.Should().HaveCount(2);

        // Each year should only be called once
        _mockBrasilApi.Verify(x => x.GetHoliday(year2025, It.IsAny<CancellationToken>()), Times.Once);
        _mockBrasilApi.Verify(x => x.GetHoliday(year2026, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetHolidaysByYearAsync_WithEmptyCache_CallsApi()
    {
        // Arrange
        var year = 2025;
        var apiResponse = new List<BrasilApiHolidayResponse>
        {
            new() { Date = new DateOnly(2025, 1, 1), Name = "Confraternização mundial", Type = BrasilApiHolidayType.National }
        };

        _mockBrasilApi
            .Setup(x => x.GetHoliday(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Verify cache is empty
        var cacheKey = $"BrasilApi_Holidays_{year}";
        _memoryCache.TryGetValue(cacheKey, out IEnumerable<HolidayDto>? _).Should().BeFalse();

        // Act
        var result = await _service.GetHolidaysByYearAsync(year);

        // Assert
        result.Should().HaveCount(1);
        _mockBrasilApi.Verify(x => x.GetHoliday(year, It.IsAny<CancellationToken>()), Times.Once);

        // Verify cache is now populated
        _memoryCache.TryGetValue(cacheKey, out IEnumerable<HolidayDto>? cachedData).Should().BeTrue();
        cachedData.Should().NotBeNull();
    }

}

