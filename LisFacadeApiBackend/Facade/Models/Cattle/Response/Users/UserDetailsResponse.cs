using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Users;

public sealed class UserDetailsResponse
{
    [JsonPropertyName("subject")]
    public string Subject { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("cphs")]
    public List<UserCphResponse> Cphs { get; init; } = [];
}