using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Reference;

public sealed class CorrespondenceAddress
{
    [JsonPropertyName("singleLineAddress")]
    public string? SingleLineAddress { get; init; }

    [JsonPropertyName("postcode")]
    [RegularExpression("^[A-Z]{1,2}[0-9][0-9A-Z]? ?[0-9][A-Z]{2}$")]
    public string? Postcode { get; init; }
}

public sealed class Haulier
{
    [JsonPropertyName("authorisationNumber")]
    public string? AuthorisationNumber { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("species")]
    public List<Species>? Species { get; init; }

    [JsonPropertyName("state")]
    public HaulierState? State { get; init; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; init; }

    [JsonPropertyName("landline")]
    public string? Landline { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("correspondenceAddress")]
    public CorrespondenceAddress? CorrespondenceAddress { get; init; }
}