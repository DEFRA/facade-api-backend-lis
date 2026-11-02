using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Request;

public sealed class HoldingRequest
{
    [JsonPropertyName("cph")]
    public string Cph { get; init; } = string.Empty;
}
