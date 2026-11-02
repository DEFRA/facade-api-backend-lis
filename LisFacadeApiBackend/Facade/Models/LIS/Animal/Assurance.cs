using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class Assurance
{
    [JsonPropertyName("currentAssuredState")]
    public CurrentAssuranceState? CurrentAssuredState { get; init; }

    [JsonPropertyName("assuranceHistory")]
    public List<AssuranceHistory>? AssuranceHistory { get; init; }
}

public sealed class AssuranceHistory
{
    [JsonPropertyName("schemeName")]
    public string? SchemeName { get; init; }

    [JsonPropertyName("certificateNumber")]
    public string? CertificateNumber { get; init; }

    [JsonPropertyName("certificationDate")]
    public DateOnly? CertificationDate { get; init; }

    [JsonPropertyName("expiryDate")]
    public DateOnly? ExpiryDate { get; init; }

    [JsonPropertyName("revocationDate")]
    public DateOnly? RevocationDate { get; init; }

    [JsonPropertyName("veterinarianName")]
    public string? VeterinarianName { get; init; }

    [JsonPropertyName("rcvsMembershipNumber")]
    public string? RcvsMembershipNumber { get; init; }

    [JsonPropertyName("vetPracticeName")]
    public string? VetPracticeName { get; init; }

    [JsonPropertyName("assuredState")]
    public AssuredState? AssuredState { get; init; }
}