using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Sex
{
    [JsonStringEnumMemberName("female")] Female,
    [JsonStringEnumMemberName("male")] Male
}