using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class AnimalRegistration
{
    [JsonPropertyName("birthDam")]
    public ParentAnimal? BirthDam { get; init; }

    [JsonPropertyName("geneticDam")]
    public ParentAnimal? GeneticDam { get; init; }

    [JsonPropertyName("sire")]
    public ParentAnimal? Sire { get; init; }

    [JsonPropertyName("site")]
    public required Site Site { get; init; }

    [JsonPropertyName("date")]
    public required DateOnly Date { get; init; }

    [JsonPropertyName("category")]
    public required RegistrationCategory Category { get; init; }

    [JsonPropertyName("identificationDate")]
    public DateOnly? IdentificationDate { get; init; }
}