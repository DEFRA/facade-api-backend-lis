using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class GetCattleForHoldingMultiSegmentAsyncUnitTests
{
    [Fact]
    public async Task Returns_all_cattle_for_known_holding()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleForHoldingMultiSegmentAsync("12", "345", "6789", null, null, null, CancellationToken.None);
        var cattle = await response.Content.ReadFromJsonAsync<List<CattleResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(["UK12345678901", "UK12345678902"], cattle!.Select(c => c.EarTag));
    }

    [Theory]
    [InlineData("uk12345678902", null, null, "UK12345678902")]
    [InlineData(null, "HOLSTEIN", null, "UK12345678901")]
    [InlineData(null, null, "male", "UK12345678902")]
    [InlineData("UK12345678901", "Holstein", "Female", "UK12345678901")]
    public async Task Filters_cattle_ignoring_case(string? earTag, string? breed, string? sex, string expectedEarTag)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleForHoldingMultiSegmentAsync("12", "345", "6789", earTag, breed, sex, CancellationToken.None);
        var cattle = await response.Content.ReadFromJsonAsync<List<CattleResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expectedEarTag, Assert.Single(cattle!).EarTag);
    }

    [Fact]
    public async Task Returns_empty_list_when_filters_match_nothing()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleForHoldingMultiSegmentAsync("12", "345", "6789", "nope", null, null, CancellationToken.None);
        var cattle = await response.Content.ReadFromJsonAsync<List<CattleResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(cattle!);
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_holding()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleForHoldingMultiSegmentAsync("99", "999", "9999", null, null, null, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, problem!.Status);
    }

    [Fact]
    public async Task Throws_argument_null_exception_for_null_segment()
    {
        var client = new FakeCattleApiClient();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.GetCattleForHoldingMultiSegmentAsync(null!, "345", "6789", null, null, null, CancellationToken.None));
    }

    [Theory]
    [InlineData("", "345", "6789")]
    [InlineData("12", " ", "6789")]
    [InlineData("12", "345", "")]
    public async Task Throws_argument_exception_for_empty_or_whitespace_segment(string county, string parish, string holding)
    {
        var client = new FakeCattleApiClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetCattleForHoldingMultiSegmentAsync(county, parish, holding, null, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.GetCattleForHoldingMultiSegmentAsync("12", "345", "6789", null, null, null, cts.Token));
    }
}
