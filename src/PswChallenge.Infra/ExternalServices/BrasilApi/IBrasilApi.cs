using PswChallenge.Infra.ExternalServices.BrasilApi.Models;
using Refit;

namespace PswChallenge.Infra.ExternalServices.BrasilApi;

public interface IBrasilApi
{
    [Get("feriados/v1/{year}")]
    Task<IEnumerable<BrasilApiHolidayResponse>> GetHoliday(int year, CancellationToken cancellationToken = default);
}