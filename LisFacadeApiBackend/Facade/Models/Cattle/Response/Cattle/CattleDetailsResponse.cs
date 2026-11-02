using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Cattle;

public sealed class CattleDetailsResponse
{
    [JsonPropertyName("earTag")]
    public string EarTag { get; init; } = string.Empty;

    [JsonPropertyName("species")]
    public string? Species { get; init; }

    [JsonPropertyName("sex")]
    public string? Sex { get; init; }

    [JsonPropertyName("dateBirth")]
    public DateOnly? DateBirth { get; init; }

    [JsonPropertyName("dateRegistered")]
    public DateOnly? DateRegistered { get; init; }

    [JsonPropertyName("dateOnCph")]
    public DateOnly? DateOnCph { get; init; }

    [JsonPropertyName("currentLocation")]
    public CattleLocationResponse? CurrentLocation { get; init; }

    [JsonPropertyName("registeredLocation")]
    public CattleLocationResponse? RegisteredLocation { get; init; }

    [JsonPropertyName("deathLocation")]
    public CattleLocationResponse? DeathLocation { get; init; }

    [JsonPropertyName("breed")]
    public string? Breed { get; init; }

    [JsonPropertyName("breedCode")]
    public string? BreedCode { get; init; }

    [JsonPropertyName("breedName")]
    public string? BreedName { get; init; }

    [JsonPropertyName("state")]
    public string? State { get; init; }

    [JsonPropertyName("restrictionStatus")]
    public string? RestrictionStatus { get; init; }

    [JsonPropertyName("damType")]
    public string? DamType { get; init; }

    [JsonPropertyName("geneticDamEarTag")]
    public string? GeneticDamEarTag { get; init; }

    [JsonPropertyName("surrogateDamEarTag")]
    public string? SurrogateDamEarTag { get; init; }

    [JsonPropertyName("sireEarTag")]
    public string? SireEarTag { get; init; }

    [JsonPropertyName("sireName")]
    public string? SireName { get; init; }
}