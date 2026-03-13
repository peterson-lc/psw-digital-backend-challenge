using FluentAssertions;
using Moq;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Queries.GetHolidays;
using MediatR;

namespace PswChallenge.Api.Tests.Endpoints;

public class HolidaysEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;

    public HolidaysEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
    }

    [Fact]
    public async Task GetHolidaysAsync_WithValidYear_ReturnsOkResult()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        var response = ApiResponseModel<IEnumerable<HolidayDto>>.Success(holidays);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _mockMediator.Object.Send(new GetHolidaysQuery(year), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHolidaysAsync_WithDifferentYears_SendsCorrectQuery()
    {
        // Arrange
        var year = 2026;
        var response = ApiResponseModel<IEnumerable<HolidayDto>>.Success(new List<HolidayDto>());

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _mockMediator.Object.Send(new GetHolidaysQuery(year), CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            x => x.Send(It.Is<GetHolidaysQuery>(q => q.Year == year), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHolidaysAsync_WithCancellationToken_PassesTokenToMediator()
    {
        // Arrange
        var year = 2025;
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        var response = ApiResponseModel<IEnumerable<HolidayDto>>.Success(new List<HolidayDto>());

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), token))
            .ReturnsAsync(response);

        // Act
        await _mockMediator.Object.Send(new GetHolidaysQuery(year), token);

        // Assert
        _mockMediator.Verify(
            x => x.Send(It.IsAny<GetHolidaysQuery>(), token),
            Times.Once);
    }
}

