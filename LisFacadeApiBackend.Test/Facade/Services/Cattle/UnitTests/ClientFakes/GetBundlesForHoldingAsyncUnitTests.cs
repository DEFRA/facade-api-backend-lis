using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class GetBundlesForHoldingAsyncUnitTests
{
    [Theory]
    [InlineData("12/345/6789")]
    [InlineData("123456789")]
    public async Task Returns_seeded_bundle_for_known_cph(string cph)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetBundlesForHoldingAsync(cph, CancellationToken.None);
        var bundles = await response.Content.ReadFromJsonAsync<List<BundleResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(FakeCattleApiData.SeededBundleId, Assert.Single(bundles!).Id);
    }

    [Fact]
    public async Task Created_bundle_is_listed_with_preserved_ids()
    {
        var client = new FakeCattleApiClient();
        var request = new RegistrationBundleRequest
        {
            ClientReference = "REF-NEW",
            Holding = new HoldingRequest { Cph = "98/765/4321" },
            Animals = [new AnimalRegistrationRequest { EarTag = "UK1", Sex = "Male", Breed = "Angus" }]
        };

        using var createResponse = await client.CreateRegistrationBundleAsync(request, CancellationToken.None);
        var created = await createResponse.Content.ReadFromJsonAsync<BundleResponse>(cancellationToken: TestContext.Current.CancellationToken);

        for (var i = 0; i < 2; i++)
        {
            using var response = await client.GetBundlesForHoldingAsync("987654321", CancellationToken.None);
            var listed = Assert.Single((await response.Content.ReadFromJsonAsync<List<BundleResponse>>(cancellationToken: TestContext.Current.CancellationToken))!);

            Assert.Equal(created!.Id, listed.Id);
            Assert.Equal(created.CreatedAt, listed.CreatedAt);
            Assert.Equal(Assert.Single(created.Animals).Id, Assert.Single(listed.Animals).Id);
        }
    }

    [Fact]
    public async Task Returns_empty_list_for_unknown_cph()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetBundlesForHoldingAsync("99/999/9999", CancellationToken.None);
        var bundles = await response.Content.ReadFromJsonAsync<List<BundleResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(bundles!);
    }

    [Fact]
    public async Task Throws_argument_null_exception_for_null_cph()
    {
        var client = new FakeCattleApiClient();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.GetBundlesForHoldingAsync(null!, CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Throws_argument_exception_for_empty_or_whitespace_cph(string cph)
    {
        var client = new FakeCattleApiClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetBundlesForHoldingAsync(cph, CancellationToken.None));
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.GetBundlesForHoldingAsync("12/345/6789", cts.Token));
    }
}
