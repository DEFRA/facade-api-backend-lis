using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class Genotype
{
    [JsonPropertyName("genotype")]
    public string? Value { get; init; }
}

public sealed class GenotypeList
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<Genotype>? Values { get; init; }
}