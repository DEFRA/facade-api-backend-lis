using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Request;

public sealed class CreateAnimalBirthRequest
{
    [JsonPropertyName("site")]
    public required CreateSiteRequest Site { get; init; }

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
