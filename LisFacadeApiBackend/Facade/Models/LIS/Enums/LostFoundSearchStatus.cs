using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LostFoundSearchStatus
{
    [JsonStringEnumMemberName("lost")] Lost,
    [JsonStringEnumMemberName("stolen")] Stolen,
    [JsonStringEnumMemberName("found")] Found
}