using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public class MovementReport
{
    [JsonPropertyName("departure")]
    public required DepartureTimestampedSite Departure { get; init; }

    [JsonPropertyName("arrival")]
    public required TimestampedSite Arrival { get; init; }

    [JsonPropertyName("number")]
    public required int Number { get; init; }

    [JsonPropertyName("receivedDate")]
    public DateOnly? ReceivedDate { get; init; }

    [JsonPropertyName("potentialMatchMovementNumbers")]
    public List<int>? PotentialMatchMovementNumbers { get; init; }

    [JsonPropertyName("batches")]
    public required List<MovementBatch> Batches { get; init; }

    [JsonPropertyName("return")]
    public MovementReturn? Return { get; init; }
}