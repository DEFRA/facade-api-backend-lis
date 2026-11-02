using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Holding;

public sealed class HoldingMarkResponse
{
    [JsonPropertyName("mark")]
    public string? Mark { get; init; }

    [JsonPropertyName("startDate")]
    public DateTimeOffset? StartDate { get; init; }

    [JsonPropertyName("endDate")]
    public DateTimeOffset? EndDate { get; init; }

    [JsonPropertyName("species")]
    public List<string> Species { get; init; } = [];
}