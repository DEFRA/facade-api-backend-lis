using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Cattle.Response.Holding;

public sealed class PaginatedResponseOfHoldingResponse
{
    [JsonPropertyName("count")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Count { get; init; }

    [JsonPropertyName("totalCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int TotalCount { get; init; }

    [JsonPropertyName("items")]
    public List<HoldingResponse> Items { get; init; } = [];
    //should be list of holding response

    [JsonPropertyName("page")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Page { get; init; }

    [JsonPropertyName("pageSize")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int PageSize { get; init; }

    [JsonPropertyName("totalPages")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int TotalPages { get; init; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; init; }

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; init; }
}
