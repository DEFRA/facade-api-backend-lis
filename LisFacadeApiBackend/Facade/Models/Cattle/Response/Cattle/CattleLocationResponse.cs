using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Cattle;

public sealed class CattleLocationResponse
{
    [JsonPropertyName("identifier")]
    public string? Identifier { get; init; }

    [JsonPropertyName("subIdentifier")]
    public string? SubIdentifier { get; init; }

    [JsonPropertyName("fullIdentifier")]
    public string? FullIdentifier { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("premisesType")]
    public string? PremisesType { get; init; }
}