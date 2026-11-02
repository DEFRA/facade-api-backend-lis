using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementConfirmation
{
    [JsonPropertyName("reportingSite")]
    public ReportingSite? ReportingSite { get; init; }

    [JsonPropertyName("action")]
    public required ConfirmationAction Action { get; init; }

    [JsonPropertyName("number")]
    public int? Number { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("rejectionReasonId")]
    public Guid? RejectionReasonId { get; init; }

    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; init; }

    [JsonPropertyName("otherReason")]
    public string? OtherReason { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; init; }
}