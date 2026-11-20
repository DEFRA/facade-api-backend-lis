using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public sealed class CreateSiteRequest
{
    [JsonPropertyName("identifiers")]
    public required List<CreateSiteIdentifierRequest> Identifiers { get; init; }
}
