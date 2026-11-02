using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Request;

public sealed class SireRegistrationRequest
{
    [JsonPropertyName("earTag")]
    public string? EarTag { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
