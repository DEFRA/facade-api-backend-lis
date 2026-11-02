using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Animal;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class BreedList
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<Breed>? Values { get; init; }
}