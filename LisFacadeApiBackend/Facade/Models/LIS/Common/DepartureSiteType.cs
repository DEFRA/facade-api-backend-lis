using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Common;

public sealed class DepartureSiteType
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("activity")]
    public SiteActivity? Activity { get; init; }
}

public sealed class SiteActivity
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}