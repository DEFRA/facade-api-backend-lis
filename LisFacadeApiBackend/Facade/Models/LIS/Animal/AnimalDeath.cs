using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class Reason
{
    [JsonPropertyName("id")]
    public Guid? Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

public sealed class AnimalDeath
{
    [JsonPropertyName("carcassCollectionSite")]
    public Site? CarcassCollectionSite { get; init; }

    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("reason")]
    public Reason? Reason { get; init; }

    [JsonPropertyName("receivedDate")]
    public DateOnly? ReceivedDate { get; init; }

    [JsonPropertyName("site")]
    public Site? Site { get; init; }

    [JsonPropertyName("tseTestRequiredFlag")]
    public bool? TseTestRequiredFlag { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; init; }

    [JsonPropertyName("updatedDateTime")]
    public DateTimeOffset? UpdatedDateTime { get; init; }
}