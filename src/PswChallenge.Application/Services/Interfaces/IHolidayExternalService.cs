using PswChallenge.Application.Models.Holidays;

namespace PswChallenge.Application.Services.Interfaces;

public interface IHolidayExternalService
{
    Task<IEnumerable<HolidayDto>> GetHolidaysByYearAsync(int year, CancellationToken cancellationToken = default);
}

