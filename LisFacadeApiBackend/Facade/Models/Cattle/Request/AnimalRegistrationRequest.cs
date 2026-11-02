using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Request;

public sealed class AnimalRegistrationRequest
{
    [JsonPropertyName("earTag")]
    public string EarTag { get; init; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; init; }

    [JsonPropertyName("sex")]
    public string? Sex { get; init; }

    [JsonPropertyName("breed")]
    public string? Breed { get; init; }

    [JsonPropertyName("dam")]
    public DamRegistrationRequest? Dam { get; init; }

    [JsonPropertyName("sire")]
    public SireRegistrationRequest? Sire { get; init; }
}
