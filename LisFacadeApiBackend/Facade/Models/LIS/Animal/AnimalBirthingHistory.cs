using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Common;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class AnimalBirthingHistory
{
    [JsonPropertyName("identifier")]
    public string? Identifier { get; init; }

    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [JsonPropertyName("sex")]
    public Sex? Sex { get; init; }

    [JsonPropertyName("birthSite")]
    public Site? BirthSite { get; init; }
}