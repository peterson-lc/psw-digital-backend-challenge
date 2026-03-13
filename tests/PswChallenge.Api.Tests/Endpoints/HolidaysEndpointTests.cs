using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Queries.GetHolidays;

namespace PswChallenge.Api.Tests.Endpoints;

public class HolidaysEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    // Same values as appsettings.json so the app's JWT validator accepts the token
    private const string JwtSecretKey = "your-very-long-secret-key-that-is-at-least-32-chars";
    private const string JwtIssuer = "PswChallenge.Api";
    private const string JwtAudience = "PswChallenge.Client";

    public HolidaysEndpointTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static string GenerateJwtToken()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task GetHolidays_WithoutAuthToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/holidays/2025");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHolidays_WithValidToken_ReturnsOk()
    {
        // Arrange
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 4, 18), "Sexta-feira Santa", HolidayType.National)
        };

        var responseModel = new HolidaysResponseModel(holidays, holidays.Count);

        _factory.MediatorMock
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponseModel<HolidaysResponseModel>.Success(responseModel));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

        // Act
        var response = await _client.GetAsync("/api/holidays/2025");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHolidays_WithValidToken_ReturnsHolidaysInBody()
    {
        // Arrange
        var holidays = new List<HolidayDto>
        {
            new(new DateOnly(2025, 1, 1), "Confraternização mundial", HolidayType.National),
            new(new DateOnly(2025, 12, 25), "Natal", HolidayType.National)
        };

        var responseModel = new HolidaysResponseModel(holidays, holidays.Count);

        _factory.MediatorMock
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponseModel<HolidaysResponseModel>.Success(responseModel));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

        // Act
        var response = await _client.GetAsync("/api/holidays/2025");
        var body = await response.Content
            .ReadFromJsonAsync<ApiResponseModel<HolidaysResponseModel>>(JsonOptions);

        // Assert
        body.Should().NotBeNull();
        body!.Succeeded.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data.Holidays.Should().HaveCount(2);
        body.Data.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetHolidays_WithValidToken_SendsQueryWithCorrectYear()
    {
        // Arrange
        var responseModel = new HolidaysResponseModel([], 0);
        _factory.MediatorMock
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponseModel<HolidaysResponseModel>.Success(responseModel));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

        // Act
        await _client.GetAsync("/api/holidays/2026");

        // Assert
        _factory.MediatorMock.Verify(
            x => x.Send(It.Is<GetHolidaysQuery>(q => q.Year == 2026), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHolidays_WithQueryParameters_SendsQueryWithFilters()
    {
        // Arrange
        var responseModel = new HolidaysResponseModel([], 0);
        _factory.MediatorMock
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponseModel<HolidaysResponseModel>.Success(responseModel));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

        // Act
        await _client.GetAsync("/api/holidays/2025?name=natal&type=National&orderBy=Name");

        // Assert
        _factory.MediatorMock.Verify(
            x => x.Send(It.Is<GetHolidaysQuery>(q =>
                q.Year == 2025 &&
                q.Name == "natal" &&
                q.Type == HolidayType.National &&
                q.OrderBy == HolidayOrderBy.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHolidays_WithDateFilter_SendsQueryWithDateFilter()
    {
        // Arrange
        var responseModel = new HolidaysResponseModel([], 0);
        _factory.MediatorMock
            .Setup(x => x.Send(It.IsAny<GetHolidaysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponseModel<HolidaysResponseModel>.Success(responseModel));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

        // Act
        await _client.GetAsync("/api/holidays/2025?date=2025-12-25");

        // Assert
        _factory.MediatorMock.Verify(
            x => x.Send(It.Is<GetHolidaysQuery>(q =>
                q.Year == 2025 &&
                q.Date == new DateOnly(2025, 12, 25)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}


