using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Error;

public sealed class ValidationErrorItem
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}
