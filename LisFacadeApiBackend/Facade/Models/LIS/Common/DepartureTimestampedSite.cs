using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Common;

public sealed class DepartureTimestampedSite
{
    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("site")]
    public required DepartureSite Site { get; init; }
}