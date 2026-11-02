using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Response.HealthCheck;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class GetDetailedHealthAsyncUnitTests
{
    [Fact]
    public async Task Returns_ok_with_healthy_response()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetDetailedHealthAsync(CancellationToken.None);
        var health = await response.Content.ReadFromJsonAsync<HealthCheckResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Healthy", health!.Status);
        Assert.Equal("Healthy", health.Entries["fake"].Status);
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.GetDetailedHealthAsync(cts.Token));
    }
}
