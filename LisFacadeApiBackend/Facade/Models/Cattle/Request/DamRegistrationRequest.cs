using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Request;

public sealed class DamRegistrationRequest
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("geneticDamEarTag")]
    public string? GeneticDamEarTag { get; init; }

    [JsonPropertyName("surrogateDamEarTag")]
    public string? SurrogateDamEarTag { get; init; }
}
