using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public class ReportAuthor
{
    [JsonPropertyName("reportingSite")]
    public ReportingSite? ReportingSite { get; init; }
}

public sealed class Confirmation : ReportAuthor
{
    [JsonPropertyName("number")]
    public double? Number { get; init; }

    [JsonPropertyName("action")]
    public required ConfirmationAction Action { get; init; }

    [JsonPropertyName("rejectionReasonId")]
    public Guid? RejectionReasonId { get; init; }

    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; init; }

    [JsonPropertyName("otherReason")]
    public string? OtherReason { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; init; }
}