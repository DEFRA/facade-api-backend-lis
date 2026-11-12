using System.Net;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LisFacadeApiBackend.Test.Utils.Http;

public class CorrelationIdMiddlewareIntegrationTests
{
    private const string RequestIdHeader = "x-cdp-request-id";
    private const string CattleApiBaseUrl = "http://cattle-api.test/";
    private const string AnimalPath = "/animals/UK111111111111";
    private const string ListPath = "/animals?species=cattle&siteIdentifier=28%2F822%2F2977";

    [Theory]
    [InlineData(AnimalPath)]
    [InlineData(ListPath)]
    public async Task Get_generates_request_id_for_cattle_api_when_caller_omits_it(string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler();
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        await client.GetAsync(path, cancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.True(Guid.TryParse(Assert.Single(request.Headers.GetValues(RequestIdHeader)), out _));
    }

    [Theory]
    [InlineData(AnimalPath, "")]
    [InlineData(AnimalPath, "   ")]
    [InlineData(ListPath, "")]
    [InlineData(ListPath, "   ")]
    public async Task Get_replaces_blank_request_id_with_generated_id(string path, string blankRequestId)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler();
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation(RequestIdHeader, blankRequestId);

        await client.SendAsync(request, cancellationToken);

        var upstreamRequest = Assert.Single(handler.Requests);
        Assert.True(Guid.TryParse(Assert.Single(upstreamRequest.Headers.GetValues(RequestIdHeader)), out _));
    }

    [Theory]
    [InlineData(AnimalPath)]
    [InlineData(ListPath)]
    public async Task Get_forwards_caller_supplied_request_id_to_cattle_api(string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        const string callerRequestId = "caller-correlation-123";
        var handler = new StubCattleApiHandler();
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add(RequestIdHeader, callerRequestId);

        await client.SendAsync(request, cancellationToken);

        var upstreamRequest = Assert.Single(handler.Requests);
        Assert.Equal(callerRequestId, Assert.Single(upstreamRequest.Headers.GetValues(RequestIdHeader)));
    }

    private static WebApplicationFactory<Program> CreateFactory(StubCattleApiHandler handler) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("CattleApi:BaseUrl", CattleApiBaseUrl);
            builder.UseSetting("CattleApi:ClientModes:GetAnimals", CattleApiClientKeys.Real);
            builder.UseSetting("CattleApi:ClientModes:GetAnimalByIdentifier", CattleApiClientKeys.Real);

            builder.ConfigureServices(services =>
                services.AddHttpClient<CattleApiClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler));
        });

    private sealed class StubCattleApiHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            var hasRequestId = request.Headers.TryGetValues(RequestIdHeader, out var values)
                && values.Any(value => !string.IsNullOrWhiteSpace(value));

            return Task.FromResult(new HttpResponseMessage(
                hasRequestId ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest));
        }
    }
}
