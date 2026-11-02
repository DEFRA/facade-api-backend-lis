using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnimalState
{
    [JsonStringEnumMemberName("registered")] Registered,
    [JsonStringEnumMemberName("dead")] Dead,
    [JsonStringEnumMemberName("lost")] Lost,
    [JsonStringEnumMemberName("stolen")] Stolen
}