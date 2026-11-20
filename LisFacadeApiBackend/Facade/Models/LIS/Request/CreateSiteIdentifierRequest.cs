using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public sealed class CreateSiteIdentifierRequest
{
    [JsonPropertyName("identifier")]
    public required string Identifier { get; init; }
}
