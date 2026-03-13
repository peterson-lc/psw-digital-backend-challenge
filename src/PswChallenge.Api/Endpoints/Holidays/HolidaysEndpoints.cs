using MediatR;
using Microsoft.AspNetCore.Mvc;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Queries.GetHolidays;

namespace PswChallenge.Api.Endpoints.Holidays;

internal static class HolidaysEndpoints
{
    extension(IEndpointRouteBuilder builder)
    {
        internal void MapHolidaysEndpoints()
        {
            builder.MapGet("api/holidays/{year}", GetHolidaysAsync)
                .WithName("GetHolidays")
                .WithTags("holidays")
                .Produces<ApiResponseModel<HolidaysResponseModel>>()
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization()
                .CacheOutput(policy => policy
                    .Expire(TimeSpan.FromHours(24))
                    .SetVaryByRouteValue("year")
                    .SetVaryByQuery("name", "type", "date", "orderBy"));
        }
    }

    private static async Task<IResult> GetHolidaysAsync(
        int year,
        [FromQuery] string? name,
        [FromQuery] HolidayType? type,
        [FromQuery] DateOnly? date,
        [FromQuery] HolidayOrderBy orderBy = HolidayOrderBy.Date,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetHolidaysQuery(year, name, type, date, orderBy);
        var response = await mediator.Send(query, cancellationToken);
        return Results.Ok(response);
    }
}

