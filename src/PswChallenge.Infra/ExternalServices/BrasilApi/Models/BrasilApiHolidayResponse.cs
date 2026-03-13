using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PswChallenge.Infra.ExternalServices.BrasilApi.Models;

[ExcludeFromCodeCoverage]
public class BrasilApiHolidayResponse
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public BrasilApiHolidayType Type { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<BrasilApiHolidayType>))]
public enum BrasilApiHolidayType
{
    [JsonStringEnumMemberName("national")]
    National,

    [JsonStringEnumMemberName("municipal")]
    Municipal
}