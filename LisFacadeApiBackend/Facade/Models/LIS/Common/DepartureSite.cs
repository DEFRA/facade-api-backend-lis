using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Common;

public sealed class DepartureSite
{
    [JsonPropertyName("type")]
    public required DepartureSiteType Type { get; init; }

    [JsonPropertyName("identifiers")]
    public required List<SiteIdentifier> Identifiers { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}