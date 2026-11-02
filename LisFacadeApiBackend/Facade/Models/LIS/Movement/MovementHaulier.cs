using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementHaulier
{
    [JsonPropertyName("type")]
    public HaulierType? Type { get; init; }

    [JsonPropertyName("authorisationNumber")]
    public string? AuthorisationNumber { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("trailerNumber")]
    public string? TrailerNumber { get; init; }

    [JsonPropertyName("vehicleRegistrationNumber")]
    public string? VehicleRegistrationNumber { get; init; }
}