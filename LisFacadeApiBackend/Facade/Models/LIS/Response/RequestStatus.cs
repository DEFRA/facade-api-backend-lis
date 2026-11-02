using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Response;

public sealed class RequestStatus
{
    [JsonPropertyName("status")]
    public AsyncRequestStatus? Status { get; init; }

    [JsonPropertyName("result")]
    public System.Text.Json.JsonElement? Result { get; init; }
}