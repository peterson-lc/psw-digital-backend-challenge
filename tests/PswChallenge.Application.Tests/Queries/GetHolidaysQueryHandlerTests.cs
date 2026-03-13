using FluentAssertions;
using Moq;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Queries.GetHolidays;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Application.Tests.Queries;

public class GetHolidaysQueryHandlerTests
{
    private readonly Mock<IHolidayExternalService> _mockHolidayService;
    private readonly GetHolidaysQueryHandler _handler;

    public GetHolidaysQueryHandlerTests()
    {
        _mockHolidayService = new Mock<IHolidayExternalService>();
        _handler = new GetHolidaysQueryHandler(_mockHolidayService.Object);
    }

    [Fact]
    public async Task Handle_WithValidYear_ReturnsSuccessResponse()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().ContainEquivalentOf(holidays[0]);
        result.Data.Should().ContainEquivalentOf(holidays[1]);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ReturnsSuccessResponseWithEmptyData()
    {
        // Arrange
        var year = 2030;
        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HolidayDto>());

        var query = new GetHolidaysQuery(year);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var year = 2025;
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, token))
            .ReturnsAsync(new List<HolidayDto>());

        var query = new GetHolidaysQuery(year);

        // Act
        await _handler.Handle(query, token);

        // Assert
        _mockHolidayService.Verify(
            x => x.GetHolidaysByYearAsync(year, token),
            Times.Once);
    }
}

