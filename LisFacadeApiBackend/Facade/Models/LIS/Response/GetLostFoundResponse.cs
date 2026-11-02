using System.Text.Json.Serialization;
using LisLostFoundModel = LisFacadeApiBackend.Facade.LIS.LostFound.LostFound;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class GetLostFoundResponse
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<LisLostFoundModel>? Values { get; init; }
}