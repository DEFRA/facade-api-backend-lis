using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementMeta
{
    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("succeeded")]
    public required int Succeeded { get; init; }

    [JsonPropertyName("failed")]
    public required int Failed { get; init; }
}