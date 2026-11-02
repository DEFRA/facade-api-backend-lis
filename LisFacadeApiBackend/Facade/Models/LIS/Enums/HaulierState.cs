using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HaulierState
{
    [JsonStringEnumMemberName("active")] Active,
    [JsonStringEnumMemberName("inactive")] Inactive,
    [JsonStringEnumMemberName("unknown")] Unknown
}