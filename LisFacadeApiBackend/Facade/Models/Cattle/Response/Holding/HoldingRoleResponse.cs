using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Holding;

public sealed class HoldingRoleResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("species")]
    public List<string> Species { get; init; } = [];
}