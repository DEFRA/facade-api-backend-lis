using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public sealed class CreateMovementDataRequest
{
    [JsonPropertyName("departure")]
    public required DepartureTimestampedSite Departure { get; init; }

    [JsonPropertyName("arrival")]
    public required TimestampedSite Arrival { get; init; }

    [JsonPropertyName("batches")]
    public required List<MovementBatch> Batches { get; init; }
}