using MediatR;
using PswChallenge.Application.Models.Base;
using PswChallenge.Application.Models.Holidays;

namespace PswChallenge.Application.Queries.GetHolidays;

public record GetHolidaysQuery(int Year) : IRequest<ApiResponseModel<IEnumerable<HolidayDto>>>;

