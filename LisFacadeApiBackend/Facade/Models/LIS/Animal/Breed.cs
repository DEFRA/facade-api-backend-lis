using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Animal;

public sealed class Breed
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("crossBreed")]
    public bool? CrossBreed { get; init; }
}