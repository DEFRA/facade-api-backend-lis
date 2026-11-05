using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public sealed class GetAnimalsRequest
{
    [JsonPropertyName("species")]
    public required Species Species { get; init; }

    [JsonPropertyName("siteIdentifier")]
    public required string SiteIdentifier { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("pageSize")]
    public int? PageSize { get; init; }

    [JsonPropertyName("order")]
    public AnimalOrder Order { get; init; } = AnimalOrder.AnimalIdentifier;

    [JsonPropertyName("sort")]
    public string Sort { get; init; } = "asc";
}
