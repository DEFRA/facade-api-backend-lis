using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Holding;

public sealed class HoldingResponse
{
    [JsonPropertyName("cph")]
    public string Cph { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("holdingType")]
    public string? HoldingType { get; init; }

    [JsonPropertyName("address")]
    public List<string> Address { get; init; } = [];

    [JsonPropertyName("keeperName")]
    public string? KeeperName { get; init; }
}