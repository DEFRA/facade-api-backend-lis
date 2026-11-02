using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Request;

public sealed class CreateUserRequest
{
    [JsonPropertyName("sub")]
    public string? Sub { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("given_name")]
    public string? GivenName { get; init; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; init; }
}
