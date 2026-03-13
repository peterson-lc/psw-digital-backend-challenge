using FluentAssertions;
using Moq;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Infra.ExternalServices.BrasilApi;
using PswChallenge.Infra.ExternalServices.BrasilApi.Models;

namespace PswChallenge.Infra.Tests.ExternalServices;

public class BrasilApiHolidayServiceTests
{
    private readonly Mock<IBrasilApi> _mockBrasilApi;
    private readonly BrasilApiHolidayService _service;

    public BrasilApiHolidayServiceTests()
    {
        _mockBrasilApi = new Mock<IBrasilApi>();
        _service = new BrasilApiHolidayService(_mockBrasilApi.Object);
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
}

