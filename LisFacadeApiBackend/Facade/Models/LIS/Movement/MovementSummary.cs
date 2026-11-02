using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementSummary
{
    [JsonPropertyName("movementNumber")]
    public double? MovementNumber { get; init; }

    [JsonPropertyName("batches")]
    public List<MovementBatch>? Batches { get; init; }

    [JsonPropertyName("departure")]
    public TimestampedSite? Departure { get; init; }

    [JsonPropertyName("arrival")]
    public TimestampedSite? Arrival { get; init; }

    [JsonPropertyName("state")]
    public MovementState? State { get; init; }
}