using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class DeathReason
{
    [JsonPropertyName("deathReasonId")]
    public Guid? DeathReasonId { get; init; }

    [JsonPropertyName("deathReason")]
    public string? DeathReasonText { get; init; }
}

public sealed class DeathReasonList
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<DeathReason>? Values { get; init; }
}