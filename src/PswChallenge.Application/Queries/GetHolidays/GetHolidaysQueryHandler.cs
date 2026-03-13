using MediatR;
using PswChallenge.Application.Helpers;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Services.Interfaces;

namespace PswChallenge.Application.Queries.GetHolidays;

public class GetHolidaysQueryHandler(IHolidayExternalService holidayExternalService)
    : IRequestHandler<GetHolidaysQuery, ApiResponseModel<HolidaysResponseModel>>
{
    public async Task<ApiResponseModel<HolidaysResponseModel>> Handle(
        GetHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var holidays = await holidayExternalService.GetHolidaysByYearAsync(request.Year, cancellationToken);

        // Apply filters
        var filteredHolidays = ApplyFilters(holidays, request);

        // Apply sorting
        var sortedHolidays = ApplySorting(filteredHolidays, request.OrderBy);

        var holidaysList = sortedHolidays.ToList();
        var response = new HolidaysResponseModel(holidaysList, holidaysList.Count);

        return ApiResponseModel<HolidaysResponseModel>.Success(response);
    }

    private static IEnumerable<HolidayDto> ApplyFilters(IEnumerable<HolidayDto> holidays, GetHolidaysQuery request)
    {
        var filtered = holidays;

        // Filter by name (case-insensitive and accent-insensitive partial match)
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedSearchTerm = StringNormalizationHelper.RemoveDiacritics(request.Name);
            filtered = filtered.Where(h =>
            {
                var normalizedHolidayName = StringNormalizationHelper.RemoveDiacritics(h.Name);
                return normalizedHolidayName.Contains(normalizedSearchTerm);
            });
        }

        // Filter by type
        if (request.Type.HasValue)
        {
            filtered = filtered.Where(h => h.Type == request.Type.Value);
        }

        // Filter by exact date
        if (request.Date.HasValue)
        {
            filtered = filtered.Where(h => h.Date == request.Date.Value);
        }

        return filtered;
    }

    private static IEnumerable<HolidayDto> ApplySorting(IEnumerable<HolidayDto> holidays, HolidayOrderBy orderBy)
    {
        return orderBy switch
        {
            HolidayOrderBy.Date => holidays.OrderBy(h => h.Date),
            HolidayOrderBy.Name => holidays.OrderBy(h => h.Name),
            HolidayOrderBy.Type => holidays.OrderBy(h => h.Type),
            _ => holidays.OrderBy(h => h.Date)
        };
    }
}

