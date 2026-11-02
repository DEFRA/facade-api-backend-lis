using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class ImportInformation : InternationalAddress
{
    [JsonPropertyName("importDeclarationFlag")]
    public required bool ImportDeclarationFlag { get; init; }

    [JsonPropertyName("healthCertificateNumber")]
    public string? HealthCertificateNumber { get; init; }

    [JsonPropertyName("ipaffsReferenceNumber")]
    public required string IpaffsReferenceNumber { get; init; }
}