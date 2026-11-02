using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Users;

public sealed class UserCphResponse
{
    [JsonPropertyName("cph")]
    public string Cph { get; init; } = string.Empty;

    [JsonPropertyName("holdingId")]
    public string? HoldingId { get; init; }

    [JsonPropertyName("holdingName")]
    public string? HoldingName { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;
}