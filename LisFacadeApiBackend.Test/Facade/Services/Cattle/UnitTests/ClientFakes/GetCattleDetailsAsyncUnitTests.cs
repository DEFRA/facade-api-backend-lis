using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class GetCattleDetailsAsyncUnitTests
{
    [Theory]
    [InlineData("UK12345678902")]
    [InlineData("uk12345678902")]
    [InlineData("Uk12345678902")]
    public async Task Returns_ok_with_cattle_matching_ear_tag_ignoring_case(string earTag)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleDetailsAsync(earTag, CancellationToken.None);
        var cattle = await response.Content.ReadFromJsonAsync<CattleDetailsResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("UK12345678902", cattle!.EarTag);
        Assert.Equal("Angus", cattle.Breed);
    }

    [Fact]
    public async Task Returns_not_found_problem_details_for_unknown_ear_tag()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleDetailsAsync("unknown", CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, problem!.Status);
    }

    [Theory]
    [InlineData("UK")]
    [InlineData("12345678901")]
    [InlineData("0")]
    [InlineData("UK12345678901 ")]
    public async Task Returns_not_found_for_non_matching_ear_tag(string earTag)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetCattleDetailsAsync(earTag, CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Throws_argument_null_exception_for_null_ear_tag()
    {
        var client = new FakeCattleApiClient();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.GetCattleDetailsAsync(null!, CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Throws_argument_exception_for_empty_or_whitespace_ear_tag(string earTag)
    {
        var client = new FakeCattleApiClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetCattleDetailsAsync(earTag, CancellationToken.None));
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.GetCattleDetailsAsync("UK12345678901", cts.Token));
    }

    [Fact]
    public async Task Fake_cattle_api_client_is_resolved_by_key()
    {
        await using var factory = new WebApplicationFactory<Program>();

        var client = factory.Services.GetRequiredKeyedService<ICattleApiClient>(CattleApiClientKeys.Fake);

        Assert.IsType<FakeCattleApiClient>(client);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(respond(request));
        }
    }
}