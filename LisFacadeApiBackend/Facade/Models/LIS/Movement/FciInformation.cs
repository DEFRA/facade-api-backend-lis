using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class FciInformation
{
    [JsonPropertyName("declarationFlag")]
    public required bool DeclarationFlag { get; init; }

    [JsonPropertyName("diseaseDetails")]
    public string? DiseaseDetails { get; init; }

    [JsonPropertyName("treatmentDetails")]
    public string? TreatmentDetails { get; init; }

    [JsonPropertyName("analysisSamples")]
    public string? AnalysisSamples { get; init; }
}