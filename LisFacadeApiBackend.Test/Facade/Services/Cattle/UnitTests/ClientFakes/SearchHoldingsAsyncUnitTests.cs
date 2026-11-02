using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class SearchHoldingsAsyncUnitTests
{
    [Fact]
    public async Task Returns_all_holdings_sorted_ascending_by_default()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, null, null, null, null, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(["12/345/6789", "23/456/7890"], result!.Items.Select(h => h.Cph));
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Theory]
    [InlineData("name", "asc", "23/456/7890")]
    [InlineData("NAME", "desc", "12/345/6789")]
    [InlineData("keeperName", "asc", "12/345/6789")]
    public async Task Sorts_by_requested_field_before_paging(string sort, string order, string expectedFirstCph)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, 1, 1, sort, order, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedFirstCph, Assert.Single(result!.Items).Cph);
    }

    [Theory]
    [InlineData(3, int.MaxValue)]
    [InlineData(int.MaxValue, 10)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public async Task Returns_empty_page_when_offset_exceeds_int_range(int page, int pageSize)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, page, pageSize, null, null, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(result!.Items);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Returns_bad_request_for_unsupported_sort_field()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, null, null, "bogus", null, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("desc")]
    [InlineData("DESC")]
    public async Task Sorts_descending_ignoring_case(string order)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, null, null, null, order, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["23/456/7890", "12/345/6789"], result!.Items.Select(h => h.Cph));
    }

    [Theory]
    [InlineData("12/345/6789")]
    [InlineData("123456789")]
    [InlineData("12 345 6789")]
    public async Task Filters_by_normalised_cph(string search)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(search, null, null, null, null, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("12/345/6789", Assert.Single(result!.Items).Cph);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Returns_empty_page_when_search_matches_nothing()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync("999999999", null, null, null, null, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(result!.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task Paginates_results()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, 2, 1, null, null, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseOfHoldingResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("23/456/7890", Assert.Single(result!.Items).Cph);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public async Task Returns_bad_request_for_zero_or_non_positive_page_or_page_size(int page, int pageSize)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.SearchHoldingsAsync(null, page, pageSize, null, null, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(400, problem!.Status);
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.SearchHoldingsAsync(null, null, null, null, null, cts.Token));
    }
}
