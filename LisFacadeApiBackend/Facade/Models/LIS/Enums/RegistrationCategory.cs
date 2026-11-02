using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegistrationCategory
{
    [JsonStringEnumMemberName("birthRegistration")] BirthRegistration,
    [JsonStringEnumMemberName("registration")] Registration
}