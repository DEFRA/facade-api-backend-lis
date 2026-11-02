using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class GetHoldingAsyncUnitTests
{
    [Theory]
    [InlineData("12", "345", "6789")]
    [InlineData("12 ", "/345", "/6789")]
    public async Task Returns_ok_with_holding_for_known_cph(string county, string parish, string holding)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetHoldingAsync(county, parish, holding, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<HoldingDetailedResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("12/345/6789", result!.Cph);
        Assert.Equal("Fake Farm", result.Name);
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_cph()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetHoldingAsync("99", "999", "9999", CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, problem!.Status);
    }

    [Theory]
    [InlineData("", "345", "6789")]
    [InlineData("12", " ", "6789")]
    [InlineData("12", "345", "")]
    [InlineData(null, "345", "6789")]
    public async Task Returns_bad_request_when_any_segment_missing(string? county, string? parish, string? holding)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetHoldingAsync(county!, parish!, holding!, CancellationToken.None);
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
            () => client.GetHoldingAsync("12", "345", "6789", cts.Token));
    }
}
