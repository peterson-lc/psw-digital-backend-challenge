namespace PswChallenge.Application.Models.Holidays;

public record HolidayDto(DateOnly Date, string Name, HolidayType Type);

public enum HolidayType
{
    National
}

