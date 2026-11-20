using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Animal;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public sealed class CreateAnimalRequest
{
    [JsonPropertyName("animal")]
    public required AnimalRecordDetails AnimalDetails { get; init; }

    [JsonPropertyName("breed")]
    public Breed? Breed { get; init; }

    [JsonPropertyName("importParents")]
    public ImportParents? ImportParents { get; init; }

    [JsonPropertyName("birth")]
    public CreateAnimalBirthRequest? Birth { get; init; }

    [JsonPropertyName("registration")]
    public required CreateAnimalRegistrationRequest Registration { get; init; }

    [JsonPropertyName("assurance")]
    public Assurance? Assurance { get; init; }
}
