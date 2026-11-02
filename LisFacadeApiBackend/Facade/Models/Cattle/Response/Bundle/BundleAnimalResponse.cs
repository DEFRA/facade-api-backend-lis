using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Bundle;

public sealed class BundleAnimalResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("submissionId")]
    public Guid SubmissionId { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("earTag")]
    public string EarTag { get; init; } = string.Empty;

    [JsonPropertyName("dateBirth")]
    public DateOnly? DateBirth { get; init; }

    [JsonPropertyName("sex")]
    public string? Sex { get; init; }

    [JsonPropertyName("breed")]
    public string? Breed { get; init; }

    [JsonPropertyName("damType")]
    public string? DamType { get; init; }

    [JsonPropertyName("damGeneticEarTag")]
    public string? DamGeneticEarTag { get; init; }

    [JsonPropertyName("damSurrogateEarTag")]
    public string? DamSurrogateEarTag { get; init; }

    [JsonPropertyName("sireEarTag")]
    public string? SireEarTag { get; init; }

    [JsonPropertyName("sireName")]
    public string? SireName { get; init; }

    [JsonPropertyName("errors")]
    public List<BundleAnimalErrorResponse> Errors { get; init; } = [];
}
