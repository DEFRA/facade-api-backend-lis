using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementMessage
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}