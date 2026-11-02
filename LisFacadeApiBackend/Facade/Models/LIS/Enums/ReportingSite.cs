using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportingSite
{
    [JsonStringEnumMemberName("departure")] Departure,
    [JsonStringEnumMemberName("arrival")] Arrival,
    [JsonStringEnumMemberName("both")] Both
}