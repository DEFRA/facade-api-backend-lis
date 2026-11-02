using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class MovementResponse
{
    [JsonPropertyName("movementNumber")]
    public double? MovementNumber { get; init; }

    [JsonPropertyName("links")]
    public MovementLinks? Links { get; init; }
}

public sealed class MovementLinks
{
    [JsonPropertyName("self")]
    public string? Self { get; init; }
}