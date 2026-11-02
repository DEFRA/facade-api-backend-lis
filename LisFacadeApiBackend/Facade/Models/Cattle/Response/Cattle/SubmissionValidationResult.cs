using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Cattle;

public sealed class SubmissionValidationResult
{
    [JsonPropertyName("submissionId")]
    public Guid SubmissionId { get; init; }

    [JsonPropertyName("isValid")]
    public bool IsValid { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("errorCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ErrorCount { get; init; }

    [JsonPropertyName("animalResults")]
    public List<SubmissionAnimalValidationResult> AnimalResults { get; init; } = [];
}
