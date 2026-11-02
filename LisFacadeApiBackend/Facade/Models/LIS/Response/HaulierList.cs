using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Reference;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class HaulierList
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<Haulier>? Values { get; init; }
}