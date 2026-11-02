using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Common;

public class InternationalAddress
{
    [JsonPropertyName("singleLineAddress")]
    public string? SingleLineAddress { get; init; }

    [JsonPropertyName("region")]
    public required string Region { get; init; }

    [JsonPropertyName("countryCode")]
    [RegularExpression("^[A-Z]{2}$")]
    public required string CountryCode { get; init; }
}