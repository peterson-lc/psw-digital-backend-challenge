using MediatR;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Application.Queries.GetHolidays;

public class GetHolidaysQueryHandler(IHolidayExternalService holidayExternalService)
    : IRequestHandler<GetHolidaysQuery, ApiResponseModel<IEnumerable<HolidayDto>>>
{
    public async Task<ApiResponseModel<IEnumerable<HolidayDto>>> Handle(
        GetHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var holidays = await holidayExternalService.GetHolidaysByYearAsync(request.Year, cancellationToken);
        return ApiResponseModel<IEnumerable<HolidayDto>>.Success(holidays);
    }
}

