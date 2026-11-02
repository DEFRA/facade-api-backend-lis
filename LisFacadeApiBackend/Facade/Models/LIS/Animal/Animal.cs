using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class Animal
{
    [JsonPropertyName("animal")]
    public required AnimalRecordDetails AnimalDetails { get; init; }

    [JsonPropertyName("breed")]
    public Breed? Breed { get; init; }

    [JsonPropertyName("currentSite")]
    public Site? CurrentSite { get; init; }

    [JsonPropertyName("importParents")]
    public ImportParents? ImportParents { get; init; }

    [JsonPropertyName("birth")]
    public AnimalBirth? Birth { get; init; }

    [JsonPropertyName("death")]
    public AnimalDeath? Death { get; init; }

    [JsonPropertyName("registration")]
    public required AnimalRegistration Registration { get; init; }

    [JsonPropertyName("homeSite")]
    public Site? HomeSite { get; init; }

    [JsonPropertyName("birthingHistory")]
    public List<AnimalBirthingHistory>? BirthingHistory { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; init; }

    [JsonPropertyName("updatedDateTime")]
    public DateTimeOffset? UpdatedDateTime { get; init; }

    [JsonPropertyName("assurance")]
    public Assurance? Assurance { get; init; }
}