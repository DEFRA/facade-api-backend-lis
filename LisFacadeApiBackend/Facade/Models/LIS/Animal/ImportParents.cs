using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class ImportParents
{
    [JsonPropertyName("sire")]
    public ParentAnimal? Sire { get; init; }

    [JsonPropertyName("geneticDam")]
    public ParentAnimal? GeneticDam { get; init; }

    [JsonPropertyName("birthDam")]
    public ParentAnimal? BirthDam { get; init; }
}