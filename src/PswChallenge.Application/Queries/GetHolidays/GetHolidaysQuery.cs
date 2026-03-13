using MediatR;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;

namespace PswChallenge.Application.Queries.GetHolidays;

public record GetHolidaysQuery(
    int Year,
    string? Name = null,
    HolidayType? Type = null,
    DateOnly? Date = null,
    HolidayOrderBy OrderBy = HolidayOrderBy.Date
) : IRequest<ApiResponseModel<HolidaysResponseModel>>;

