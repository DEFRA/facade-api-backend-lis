using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Holding;

public sealed class CattleResponse
{
    [JsonPropertyName("earTag")]
    public string EarTag { get; init; } = string.Empty;

    [JsonPropertyName("dateBirth")]
    public DateOnly? DateBirth { get; init; }

    [JsonPropertyName("dateOnCph")]
    public DateOnly? DateOnCph { get; init; }

    [JsonPropertyName("sex")]
    public string? Sex { get; init; }

    [JsonPropertyName("breed")]
    public string? Breed { get; init; }

    [JsonPropertyName("breedCode")]
    public string? BreedCode { get; init; }

    [JsonPropertyName("breedName")]
    public string? BreedName { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<CattleErrorResponse> Errors { get; init; } = [];
}