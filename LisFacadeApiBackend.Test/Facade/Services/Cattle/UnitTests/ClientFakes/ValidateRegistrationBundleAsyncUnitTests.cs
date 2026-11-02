using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class ValidateRegistrationBundleAsyncUnitTests
{
    [Fact]
    public async Task Returns_animal_results_matching_created_bundle_animals()
    {
        var client = new FakeCattleApiClient();
        var request = new RegistrationBundleRequest
        {
            ClientReference = "REF-1",
            Holding = new HoldingRequest { Cph = "12/345/6789" },
            Animals =
            [
                new AnimalRegistrationRequest { EarTag = "UK1", Sex = "Male", Breed = "Angus" },
                new AnimalRegistrationRequest { EarTag = "UK2", Sex = "Female", Breed = "Hereford" }
            ]
        };

        using var createResponse = await client.CreateRegistrationBundleAsync(request, CancellationToken.None);
        var bundle = await createResponse.Content.ReadFromJsonAsync<LisFacadeApiBackend.Facade.Cattle.Response.Bundle.BundleResponse>(cancellationToken: TestContext.Current.CancellationToken);
        using var response = await client.ValidateRegistrationBundleAsync(bundle!.Id.ToString(), CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<SubmissionValidationResult>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            bundle.Animals.Select(a => (a.Id, a.EarTag)),
            result!.AnimalResults.Select(a => (a.AnimalId, a.EarTag)));
        Assert.Equal(["UK1", "UK2"], result.AnimalResults.Select(a => a.EarTag));
    }

    [Fact]
    public async Task Returns_valid_result_for_seeded_bundle()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.ValidateRegistrationBundleAsync(FakeCattleApiData.SeededBundleId.ToString(), CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<SubmissionValidationResult>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(FakeCattleApiData.SeededBundleId, result!.SubmissionId);
        Assert.True(result.IsValid);
        Assert.Equal("Valid", result.Status);
        Assert.Equal(0, result.ErrorCount);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData("22222222-2222-2222-2222-222222222222")]
    public async Task Returns_not_found_for_invalid_or_unknown_id(string id)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.ValidateRegistrationBundleAsync(id, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, problem!.Status);
    }

    [Fact]
    public async Task Returns_not_found_for_null_id()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.ValidateRegistrationBundleAsync(null!, CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.ValidateRegistrationBundleAsync(FakeCattleApiData.SeededBundleId.ToString(), cts.Token));
    }
}
