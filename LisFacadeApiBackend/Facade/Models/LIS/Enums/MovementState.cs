using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovementState
{
    [JsonStringEnumMemberName("awaitingConfirmation")] AwaitingConfirmation,
    [JsonStringEnumMemberName("awaitingReview")] AwaitingReview,
    [JsonStringEnumMemberName("blocked")] Blocked,
    [JsonStringEnumMemberName("complete")] Complete,
    [JsonStringEnumMemberName("cancelled")] Cancelled,
    [JsonStringEnumMemberName("inDispute")] InDispute,
    [JsonStringEnumMemberName("preNotified")] PreNotified,
    [JsonStringEnumMemberName("resubmit")] Resubmit
}