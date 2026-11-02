using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AsyncRequestStatus
{
    [JsonStringEnumMemberName("Pending")] Pending,
    [JsonStringEnumMemberName("Completed")] Completed,
    [JsonStringEnumMemberName("Failed")] Failed
}