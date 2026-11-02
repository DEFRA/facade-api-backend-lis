using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.Cattle.Error;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Cattle;

public sealed class SubmissionAnimalValidationResult
{
    [JsonPropertyName("animalId")]
    public Guid AnimalId { get; init; }

    [JsonPropertyName("earTag")]
    public string EarTag { get; init; } = string.Empty;

    [JsonPropertyName("isValid")]
    public bool IsValid { get; init; }

    [JsonPropertyName("errors")]
    public List<ValidationErrorItem> Errors { get; init; } = [];
}
