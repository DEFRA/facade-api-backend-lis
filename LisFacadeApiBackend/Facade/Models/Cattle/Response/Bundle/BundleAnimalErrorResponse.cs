using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Bundle;

public sealed class BundleAnimalErrorResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("animalId")]
    public Guid AnimalId { get; init; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; init; } = string.Empty;

    [JsonPropertyName("errorText")]
    public string ErrorText { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }
}
