using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Animal;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class GetAnimalsResponse
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<AnimalSummary>? Values { get; init; }
}