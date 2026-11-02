using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class Movements
{
    [JsonPropertyName("values")]
    public required List<MovementSummary> Values { get; init; }

    [JsonPropertyName("errors")]
    public required List<MovementError> Errors { get; init; }

    [JsonPropertyName("meta")]
    public required MovementMeta Meta { get; init; }
}