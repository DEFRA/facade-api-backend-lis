using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class AnimalResponse
{
    [JsonPropertyName("identifier")]
    public string? Identifier { get; init; }

    [JsonPropertyName("links")]
    public AnimalLinks? Links { get; init; }
}

public sealed class AnimalLinks
{
    [JsonPropertyName("self")]
    public string? Self { get; init; }
}