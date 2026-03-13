using MediatR;
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
            builder.MapGet("/holidays/{year}", GetHolidaysAsync)
                .WithName("GetHolidays")
                .WithTags("holidays")
                .Produces<ApiResponseModel<IEnumerable<HolidayDto>>>()
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();
        }
    }

    private static async Task<IResult> GetHolidaysAsync(
        int year,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetHolidaysQuery(year), cancellationToken);
        return Results.Ok(response);
    }
}

