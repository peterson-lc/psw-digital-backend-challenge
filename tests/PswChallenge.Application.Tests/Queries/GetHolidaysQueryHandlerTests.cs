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
        result.Data.Should().NotBeNull();
        result.Data.Holidays.Should().HaveCount(2);
        result.Data.Total.Should().Be(2);
        result.Data.Holidays.Should().ContainEquivalentOf(holidays[0]);
        result.Data.Holidays.Should().ContainEquivalentOf(holidays[1]);
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
        result.Data.Should().NotBeNull();
        result.Data.Holidays.Should().BeEmpty();
        result.Data.Total.Should().Be(0);
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

    [Fact]
    public async Task Handle_WithNameFilter_ReturnsFilteredHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National),
            new(new DateOnly(2025, 4, 21), "Tiradentes", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "natal");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Total.Should().Be(1);
        result.Data.Holidays.First().Name.Should().Be("Natal");
    }

    [Fact]
    public async Task Handle_WithTypeFilter_ReturnsFilteredHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 6, 15), "Aniversário da Cidade", HolidayType.Municipal),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Type: HolidayType.Municipal);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Total.Should().Be(1);
        result.Data.Holidays.First().Type.Should().Be(HolidayType.Municipal);
    }

    [Fact]
    public async Task Handle_WithDateFilter_ReturnsFilteredHolidays()
    {
        // Arrange
        var year = 2025;
        var targetDate = new DateOnly(2025, 12, 25);
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(targetDate, "Natal", HolidayType.National),
            new(new DateOnly(2025, 4, 21), "Tiradentes", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Date: targetDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Total.Should().Be(1);
        result.Data.Holidays.First().Date.Should().Be(targetDate);
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ReturnsFilteredHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 6, 15), "Aniversário da Cidade", HolidayType.Municipal),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "natal", Type: HolidayType.National);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Total.Should().Be(1);
        result.Data.Holidays.First().Name.Should().Be("Natal");
        result.Data.Holidays.First().Type.Should().Be(HolidayType.National);
    }

    [Fact]
    public async Task Handle_WithOrderByDate_ReturnsSortedHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National),
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 4, 21), "Tiradentes", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, OrderBy: HolidayOrderBy.Date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(3);
        result.Data.Holidays.First().Date.Should().Be(new DateOnly(2025, 1, 1));
        result.Data.Holidays.Last().Date.Should().Be(new DateOnly(2025, 12, 25));
    }

    [Fact]
    public async Task Handle_WithOrderByName_ReturnsSortedHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National),
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 4, 21), "Tiradentes", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, OrderBy: HolidayOrderBy.Name);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(3);
        result.Data.Holidays.First().Name.Should().Be("Confraternização mundial");
        result.Data.Holidays.Last().Name.Should().Be("Tiradentes");
    }

    [Fact]
    public async Task Handle_WithOrderByType_ReturnsSortedHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 6, 15), "Aniversário da Cidade", HolidayType.Municipal),
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 7, 20), "Feriado Municipal", HolidayType.Municipal)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, OrderBy: HolidayOrderBy.Type);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(3);
        // National (0) comes before Municipal (1) in enum ordering
        result.Data.Holidays.First().Type.Should().Be(HolidayType.National);
        result.Data.Holidays.Last().Type.Should().Be(HolidayType.Municipal);
    }

    [Fact]
    public async Task Handle_WithNameFilterWithoutAccents_MatchesHolidayWithAccents()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 4, 20), "Páscoa", HolidayType.National),
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "pascoa"); // Without accent

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Holidays.First().Name.Should().Be("Páscoa"); // Original name preserved
        result.Data.Total.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNameFilterWithAccents_MatchesHolidayWithoutAccents()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 4, 20), "Pascoa", HolidayType.National), // Without accent
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "Páscoa"); // With accent

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Holidays.First().Name.Should().Be("Pascoa"); // Original name preserved
        result.Data.Total.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNameFilterConfraternizacao_MatchesConfraternizacaoWithCedilla()
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

        var query = new GetHolidaysQuery(year, Name: "confraternizacao"); // Without ç

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Holidays.First().Name.Should().Be("Confraternização mundial"); // Original preserved
        result.Data.Total.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithUppercaseNameFilter_MatchesLowercaseHoliday()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National),
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "NATAL"); // Uppercase

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        result.Data.Holidays.First().Name.Should().Be("Natal"); // Original case preserved
        result.Data.Total.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPartialNameAndAccents_MatchesMultipleHolidays()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 6, 15), "São João", HolidayType.Municipal),
            new(new DateOnly(2025, 6, 24), "São Pedro", HolidayType.Municipal),
            new(new DateOnly(2025, 1, 25), "Aniversário de São Paulo", HolidayType.Municipal),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "sao"); // Without tilde

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(3); // São João, São Pedro, Aniversário de São Paulo
        result.Data.Total.Should().Be(3);
        result.Data.Holidays.Should().Contain(h => h.Name == "São João");
        result.Data.Holidays.Should().Contain(h => h.Name == "São Pedro");
        result.Data.Holidays.Should().Contain(h => h.Name == "Aniversário de São Paulo");
    }

    [Fact]
    public async Task Handle_WithAccentInsensitiveSearch_PreservesOriginalNames()
    {
        // Arrange
        var year = 2025;
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 4, 20), "Páscoa", HolidayType.National),
            new(new DateOnly(2025, 9, 7), "Independência do Brasil", HolidayType.National),
            new(new DateOnly(2025, 11, 15), "Proclamação da República", HolidayType.National)
        };

        _mockHolidayService
            .Setup(x => x.GetHolidaysByYearAsync(year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        var query = new GetHolidaysQuery(year, Name: "republica"); // Without accent

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Holidays.Should().HaveCount(1);
        // Verify original name with accents is preserved in response
        result.Data.Holidays.First().Name.Should().Be("Proclamação da República");
        result.Data.Total.Should().Be(1);
    }
}
