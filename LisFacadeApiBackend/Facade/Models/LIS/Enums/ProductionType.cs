using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductionType
{
    [JsonStringEnumMemberName("meat")] Meat,
    [JsonStringEnumMemberName("dairy")] Dairy,
    [JsonStringEnumMemberName("beef")] Beef
}