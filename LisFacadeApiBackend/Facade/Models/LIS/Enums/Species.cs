using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Species
{
    [JsonStringEnumMemberName("bison")] Bison,
    [JsonStringEnumMemberName("buffalo")] Buffalo,
    [JsonStringEnumMemberName("cattle")] Cattle,
    [JsonStringEnumMemberName("deer")] Deer,
    [JsonStringEnumMemberName("goats")] Goats,
    [JsonStringEnumMemberName("sheep")] Sheep
}