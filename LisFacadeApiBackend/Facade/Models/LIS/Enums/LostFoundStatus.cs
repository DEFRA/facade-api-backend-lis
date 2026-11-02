using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LostFoundStatus
{
    [JsonStringEnumMemberName("found")] Found,
    [JsonStringEnumMemberName("lost")] Lost,
    [JsonStringEnumMemberName("stolen")] Stolen
}