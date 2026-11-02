using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.HealthCheck;

public sealed class HealthCheckResponse
{
    [JsonPropertyName("status")]
    public required string Status { get; init; } = string.Empty;

    [JsonPropertyName("totalDurationSeconds")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required double TotalDurationSeconds { get; init; }

    [JsonPropertyName("entries")]
    public required Dictionary<string, ComponentHealthResult> Entries { get; init; } = [];
}