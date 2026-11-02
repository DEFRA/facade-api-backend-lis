using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementReportHistory : MovementReport
{
    [JsonPropertyName("reportingSite")]
    public ReportingSite? ReportingSite { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; init; }

    [JsonPropertyName("updatedDateTime")]
    public DateTimeOffset? UpdatedDateTime { get; init; }
}