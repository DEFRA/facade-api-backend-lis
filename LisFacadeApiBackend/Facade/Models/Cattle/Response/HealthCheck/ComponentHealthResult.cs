using System.Text.Json;
using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.HealthCheck;
    
public sealed class ComponentHealthResult
{
    [JsonPropertyName("status")]
    public required string Status { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("durationSeconds")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double DurationSeconds { get; init; }

    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }
}