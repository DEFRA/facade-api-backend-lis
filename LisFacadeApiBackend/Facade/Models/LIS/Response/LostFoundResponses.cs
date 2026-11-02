using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class LostFoundResponse
{
    [JsonPropertyName("id")]
    public Guid? Id { get; init; }

    [JsonPropertyName("links")]
    public LostFoundLinks? Links { get; init; }
}

public sealed class LostFoundLinks
{
    [JsonPropertyName("self")]
    public string? Self { get; init; }
}