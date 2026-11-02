using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class AnimalRecordDetails
{
    [JsonPropertyName("identifier")]
    public required string Identifier { get; init; }

    [JsonPropertyName("species")]
    public required Species Species { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("sex")]
    public required Sex Sex { get; init; }

    [JsonPropertyName("genotype")]
    public string? Genotype { get; init; }

    [JsonPropertyName("originalIdentifier")]
    public string? OriginalIdentifier { get; init; }

    [JsonPropertyName("productionType")]
    public ProductionType? ProductionType { get; init; }

    [JsonPropertyName("state")]
    public AnimalState? State { get; init; }
}