using Microsoft.Extensions.Caching.Memory;
using PswChallenge.Application.Models.Holidays;
using PswChallenge.Application.Services.Interfaces;
using PswChallenge.Infra.ExternalServices.BrasilApi.Models;

namespace PswChallenge.Infra.ExternalServices.BrasilApi;

public class BrasilApiHolidayService(IBrasilApi brasilApi, IMemoryCache memoryCache) : IHolidayExternalService
{
    private const string CacheKeyPrefix = "BrasilApi_Holidays_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public async Task<IEnumerable<HolidayDto>> GetHolidaysByYearAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{year}";

        // Try to get from cache first (cache-aside pattern)
        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<HolidayDto>? cachedHolidays) && cachedHolidays != null)
        {
            return cachedHolidays;
        }

        // Cache miss - call the API
        var holidays = await brasilApi.GetHoliday(year, cancellationToken);
        var mappedHolidays = holidays.Select(h => new HolidayDto(h.Date, h.Name, MapType(h.Type))).ToList();

        // Store in cache with 24-hour expiration
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheExpiration);

        memoryCache.Set(cacheKey, mappedHolidays, cacheEntryOptions);

        return mappedHolidays;
    }

    private static HolidayType MapType(BrasilApiHolidayType type) => type switch
    {
        BrasilApiHolidayType.National => HolidayType.National,
        BrasilApiHolidayType.Municipal => HolidayType.Municipal,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

