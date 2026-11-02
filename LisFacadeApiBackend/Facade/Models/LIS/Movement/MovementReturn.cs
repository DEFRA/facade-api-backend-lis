using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementReturn
{
    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("number")]
    public int? Number { get; init; }
}