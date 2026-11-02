using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class RejectionReason
{
    [JsonPropertyName("rejectionReasonId")]
    public Guid? RejectionReasonId { get; init; }

    [JsonPropertyName("rejectionReason")]
    public string? RejectionReasonText { get; init; }
}

public sealed class RejectionReasonList
{
    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("values")]
    public List<RejectionReason>? Values { get; init; }
}