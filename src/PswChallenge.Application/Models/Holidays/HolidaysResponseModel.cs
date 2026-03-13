using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Models.Holidays;

[ExcludeFromCodeCoverage]
public record HolidaysResponseModel(IEnumerable<HolidayDto> Holidays, int Total);

