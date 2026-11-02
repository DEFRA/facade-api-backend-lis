using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Common;

public sealed class SiteIdentifier
{
    [JsonPropertyName("identifier")]
    public required string Identifier { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }
}