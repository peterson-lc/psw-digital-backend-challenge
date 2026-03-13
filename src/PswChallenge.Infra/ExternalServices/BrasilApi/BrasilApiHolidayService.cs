using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Services.Interfaces;
using PswChallenge.Infra.ExternalServices.BrasilApi.Models;

namespace PswChallenge.Infra.ExternalServices.BrasilApi;

public class BrasilApiHolidayService(IBrasilApi brasilApi) : IHolidayExternalService
{
    public async Task<IEnumerable<HolidayDto>> GetHolidaysByYearAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        var holidays = await brasilApi.GetHoliday(year, cancellationToken);
        return holidays.Select(h => new HolidayDto(h.Date, h.Name, MapType(h.Type)));
    }

    private static HolidayType MapType(BrasilApiHolidayType type) => type switch
    {
        BrasilApiHolidayType.National => HolidayType.National,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

