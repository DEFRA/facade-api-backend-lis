using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class AnimalBirth
{
    [JsonPropertyName("site")]
    public required Site Site { get; init; }

    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("year")]
    public int? Year { get; init; }

    [JsonPropertyName("mark")]
    public string? Mark { get; init; }

    [JsonPropertyName("assistedBirthFlag")]
    public required bool AssistedBirthFlag { get; init; }

    [JsonPropertyName("multipleBirthsFlag")]
    public required bool MultipleBirthsFlag { get; init; }

    [JsonPropertyName("embryoTransferFlag")]
    public required bool EmbryoTransferFlag { get; init; }
}