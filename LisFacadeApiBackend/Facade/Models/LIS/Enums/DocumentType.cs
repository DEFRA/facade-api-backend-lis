using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DocumentType
{
    [JsonStringEnumMemberName("importHealthCertificate")] ImportHealthCertificate,
    [JsonStringEnumMemberName("circularMovementPermit")] CircularMovementPermit
}