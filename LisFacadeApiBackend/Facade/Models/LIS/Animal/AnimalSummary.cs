using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class AnimalSummaryAnimal
{
    [JsonPropertyName("identifier")]
    public required string Identifier { get; init; }

    [JsonPropertyName("species")]
    public required Species Species { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("sex")]
    public required Sex Sex { get; init; }

    [JsonPropertyName("state")]
    public AnimalState? State { get; init; }
}

public sealed class AnimalSummary
{
    [JsonPropertyName("animal")]
    public AnimalSummaryAnimal? Animal { get; init; }

    [JsonPropertyName("breed")]
    public Breed? Breed { get; init; }
}