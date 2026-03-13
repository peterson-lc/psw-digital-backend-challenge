using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Models.Holidays;

[ExcludeFromCodeCoverage]
public record HolidayDto(DateOnly Date, string Name, HolidayType Type);

public enum HolidayType
{
    National,
    Municipal
}

