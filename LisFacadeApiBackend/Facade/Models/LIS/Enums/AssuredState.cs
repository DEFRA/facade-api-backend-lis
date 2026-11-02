using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssuredState
{
    [JsonStringEnumMemberName("assured")] Assured,
    [JsonStringEnumMemberName("expired")] Expired,
    [JsonStringEnumMemberName("revoked")] Revoked
}