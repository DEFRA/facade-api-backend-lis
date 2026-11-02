using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Common;

public sealed class Site
{
    [JsonPropertyName("type")]
    public required SiteType Type { get; init; }

    [JsonPropertyName("identifiers")]
    public required List<SiteIdentifier> Identifiers { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}