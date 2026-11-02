using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Bundle;

public sealed class BundleResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("clientReference")]
    public string ClientReference { get; init; } = string.Empty;

    [JsonPropertyName("countyParishHolding")]
    public string CountyParishHolding { get; init; } = string.Empty;

    [JsonPropertyName("submittedBy")]
    public string SubmittedBy { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("animals")]
    public List<BundleAnimalResponse> Animals { get; init; } = [];
}
