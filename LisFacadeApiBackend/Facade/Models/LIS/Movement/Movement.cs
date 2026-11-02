using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class Movement : MovementReport
{
    [JsonPropertyName("matchedMovementNumbers")]
    public List<int>? MatchedMovementNumbers { get; init; }

    [JsonPropertyName("movementKind")]
    public required MovementKind MovementKind { get; init; }

    [JsonPropertyName("inBusinessMoveFlag")]
    public bool? InBusinessMoveFlag { get; init; }

    [JsonPropertyName("state")]
    public required MovementState State { get; init; }

    [JsonPropertyName("confirmations")]
    public List<MovementConfirmation>? Confirmations { get; init; }

    [JsonPropertyName("movementReports")]
    public required List<MovementReportHistory> MovementReports { get; init; }

    [JsonPropertyName("haulier")]
    public MovementHaulier? Haulier { get; init; }

    [JsonPropertyName("exportInformation")]
    public InternationalAddress? ExportInformation { get; init; }

    [JsonPropertyName("importInformation")]
    public ImportInformation? ImportInformation { get; init; }

    [JsonPropertyName("fciInformation")]
    public FciInformation? FciInformation { get; init; }

    [JsonPropertyName("documents")]
    public List<MovementDocument>? Documents { get; init; }

    [JsonPropertyName("createdDateTime")]
    public required DateTimeOffset CreatedDateTime { get; init; }

    [JsonPropertyName("updatedDateTime")]
    public required DateTimeOffset UpdatedDateTime { get; init; }

    [JsonPropertyName("messages")]
    public List<MovementMessage>? Messages { get; init; }
}