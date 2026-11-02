using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.LostFound;

public sealed class LostFound
{
    [JsonPropertyName("id")]
    public Guid? Id { get; init; }

    [JsonPropertyName("identifier")]
    public required string Identifier { get; init; }

    [JsonPropertyName("site")]
    public Site? Site { get; init; }

    [JsonPropertyName("status")]
    public required LostFoundStatus Status { get; init; }

    [JsonPropertyName("eventDate")]
    public required DateOnly EventDate { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; init; }

    [JsonPropertyName("crimeReferenceNumber")]
    public string? CrimeReferenceNumber { get; init; }

    [JsonPropertyName("foundDead")]
    public bool? FoundDead { get; init; }
}