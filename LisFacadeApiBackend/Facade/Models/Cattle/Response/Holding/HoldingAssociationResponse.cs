using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Holding;

public sealed class HoldingAssociationResponse
{
    [JsonPropertyName("customerNumber")]
    public string? CustomerNumber { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("partyType")]
    public string? PartyType { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; init; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; init; }

    [JsonPropertyName("address")]
    public List<string> Address { get; init; } = [];

    [JsonPropertyName("roles")]
    public List<HoldingRoleResponse> Roles { get; init; } = [];
}