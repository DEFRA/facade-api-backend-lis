using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class AsyncAcceptedResponse
{
    [JsonPropertyName("id")]
    public Guid? Id { get; init; }

    [JsonPropertyName("requestStatus")]
    public AsyncRequestStatus? RequestStatus { get; init; }

    [JsonPropertyName("requestDate")]
    public DateTimeOffset? RequestDate { get; init; }

    [JsonPropertyName("requestStatusUrl")]
    public required string RequestStatusUrl { get; init; }
}