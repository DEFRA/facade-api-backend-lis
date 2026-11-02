using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class CreateRegistrationBundleAsyncUnitTests
{
    [Fact]
    public async Task Returns_created_with_bundle_built_from_request()
    {
        var client = new FakeCattleApiClient();
        var request = new RegistrationBundleRequest
        {
            ClientReference = "REF-1",
            Holding = new HoldingRequest { Cph = "12/345/6789" },
            SubmittedBy = "Tester",
            Animals = [new AnimalRegistrationRequest { EarTag = "UK1", Sex = "Male", Breed = "Angus" }]
        };

        using var response = await client.CreateRegistrationBundleAsync(request, CancellationToken.None);
        var bundle = await response.Content.ReadFromJsonAsync<BundleResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotEqual(Guid.Empty, bundle!.Id);
        Assert.Equal("REF-1", bundle.ClientReference);
        Assert.Equal("12/345/6789", bundle.CountyParishHolding);
        Assert.Equal("Tester", bundle.SubmittedBy);
        Assert.Equal("Pending", bundle.Status);
        var animal = Assert.Single(bundle.Animals);
        Assert.Equal("UK1", animal.EarTag);
        Assert.Equal("Angus", animal.Breed);
    }

    [Fact]
    public async Task Defaults_submitted_by_to_empty_string_when_null()
    {
        var client = new FakeCattleApiClient();
        var request = new RegistrationBundleRequest
        {
            ClientReference = "REF-1",
            Holding = new HoldingRequest { Cph = "12/345/6789" }
        };

        using var response = await client.CreateRegistrationBundleAsync(request, CancellationToken.None);
        var bundle = await response.Content.ReadFromJsonAsync<BundleResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(string.Empty, bundle!.SubmittedBy);
    }

    [Fact]
    public async Task Created_bundle_can_subsequently_be_validated()
    {
        var client = new FakeCattleApiClient();
        var request = new RegistrationBundleRequest
        {
            ClientReference = "REF-1",
            Holding = new HoldingRequest { Cph = "12/345/6789" }
        };

        using var createResponse = await client.CreateRegistrationBundleAsync(request, CancellationToken.None);
        var bundle = await createResponse.Content.ReadFromJsonAsync<BundleResponse>(cancellationToken: TestContext.Current.CancellationToken);
        using var validateResponse = await client.ValidateRegistrationBundleAsync(bundle!.Id.ToString(), CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
    }

    [Fact]
    public async Task Returns_bad_request_for_null_request()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.CreateRegistrationBundleAsync(null!, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(400, problem!.Status);
    }

    [Theory]
    [InlineData("", "12/345/6789")]
    [InlineData(" ", "12/345/6789")]
    [InlineData("REF-1", "")]
    [InlineData("REF-1", " ")]
    [InlineData("REF-1", null)]
    public async Task Returns_bad_request_when_client_reference_or_cph_missing(string clientReference, string? cph)
    {
        var client = new FakeCattleApiClient();
        var request = new RegistrationBundleRequest
        {
            ClientReference = clientReference,
            Holding = cph is null ? null : new HoldingRequest { Cph = cph }
        };

        using var response = await client.CreateRegistrationBundleAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.CreateRegistrationBundleAsync(new RegistrationBundleRequest(), cts.Token));
    }
}
