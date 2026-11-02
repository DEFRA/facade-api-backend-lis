using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Request;

public sealed class RegistrationBundleRequest
{
    [JsonPropertyName("clientReference")]
    public string ClientReference { get; init; } = string.Empty;

    [JsonPropertyName("holding")]
    public HoldingRequest? Holding { get; init; }

    [JsonPropertyName("submittedBy")]
    public string? SubmittedBy { get; init; }

    [JsonPropertyName("animals")]
    public List<AnimalRegistrationRequest> Animals { get; init; } = [];
}
