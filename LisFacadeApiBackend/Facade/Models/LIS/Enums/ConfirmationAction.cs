using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConfirmationAction
{
    [JsonStringEnumMemberName("confirmOff")] ConfirmOff,
    [JsonStringEnumMemberName("accept")] Accept,
    [JsonStringEnumMemberName("reject")] Reject
}