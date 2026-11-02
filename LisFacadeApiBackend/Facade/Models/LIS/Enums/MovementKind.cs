using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovementKind
{
    [JsonStringEnumMemberName("regular")] Regular,
    [JsonStringEnumMemberName("circular")] Circular,
    [JsonStringEnumMemberName("export")] Export,
    [JsonStringEnumMemberName("import")] Import
}