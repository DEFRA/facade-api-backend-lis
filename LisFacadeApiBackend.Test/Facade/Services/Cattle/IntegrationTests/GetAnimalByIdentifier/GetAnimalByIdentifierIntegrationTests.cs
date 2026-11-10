using System.Net;
using System.Net.Http.Json;
using System.Text;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.IntegrationTests.GetAnimalByIdentifier;

public class GetAnimalByIdentifierIntegrationTests
{
    private static readonly WebApplicationFactory<Program> s_fakeCattleFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        builder.ConfigureServices(services =>
            services.AddKeyedSingleton<ICattleApiClient>(
                CattleApiClientKeys.Real,
                (sp, _) => sp.GetRequiredKeyedService<ICattleApiClient>(CattleApiClientKeys.Fake))));

    private const string KnownIdentifier = "UK111111111111";
    private const string CattleApiBaseUrl = "http://cattle-api.test/";
    private const string ApiKey = "test-api-key";

    [Theory]
    [InlineData("UK12345678901")]
    [InlineData("UK12345678902")]
    [InlineData("UK12345678903")]
    [InlineData("UK12345678904")]
    [InlineData("UK12345678905")]
    [InlineData("UK12345678906")]
    [InlineData("UK12345678907")]
    [InlineData("UK12345678908")]
    [InlineData("UK12345678909")]
    [InlineData("UK12345678910")]
    public async Task Get_returns_200_for_every_seeded_fake_identifier(string identifier)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = s_fakeCattleFactory.CreateClient();

        var response = await client.GetAsync($"/animals/{identifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.Equal(identifier, animal!.AnimalDetails.Identifier);
    }

    [Fact]
    public async Task Get_returns_200_with_translated_animal_for_known_identifier()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson(FullyPopulatedCattle()));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"/v1/cattle/{KnownIdentifier}", request.RequestUri?.AbsolutePath);

        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.NotNull(animal);
        Assert.Equal(KnownIdentifier, animal.AnimalDetails.Identifier);
        Assert.Equal(Species.Cattle, animal.AnimalDetails.Species);
        Assert.Equal(Sex.Female, animal.AnimalDetails.Sex);
        Assert.Equal(AnimalState.Registered, animal.AnimalDetails.State);

        Assert.Equal("AA", animal.Breed?.Code);
        Assert.Equal("Aberdeen Angus", animal.Breed?.Name);

        Assert.Equal("Current Farm", animal.CurrentSite?.Name);
        Assert.Equal("AH", animal.CurrentSite?.Type.Type);
        Assert.Equal("11/111/1111", Assert.Single(animal.CurrentSite!.Identifiers).Identifier);

        Assert.Equal("Abattoir", animal.Death?.Site?.Name);
        Assert.Equal("33/333/3333", Assert.Single(animal.Death!.Site!.Identifiers).Identifier);

        Assert.Equal("UK000000000001", animal.ImportParents?.Sire?.Identifier);
        Assert.Equal("Big Bull", animal.ImportParents?.Sire?.Name);
        Assert.Equal("UK000000000002", animal.ImportParents?.GeneticDam?.Identifier);
        Assert.Equal("UK000000000003", animal.ImportParents?.BirthDam?.Identifier);

        Assert.Equal(new DateOnly(2024, 3, 15), animal.Registration.Date);
        Assert.Equal("Birth Farm", animal.Registration.Site.Name);
        Assert.Equal("UK000000000001", animal.Registration.Sire?.Identifier);
        Assert.Equal("UK000000000002", animal.Registration.GeneticDam?.Identifier);
        Assert.Equal("UK000000000003", animal.Registration.BirthDam?.Identifier);
    }

    [Fact]
    public async Task Get_forwards_identifier_case_unchanged_to_cattle_api()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        await client.GetAsync($"/animals/{KnownIdentifier.ToLowerInvariant()}", cancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal($"/v1/cattle/{KnownIdentifier.ToLowerInvariant()}", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task Get_escapes_url_decoded_identifier_when_calling_cattle_api()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        await client.GetAsync("/animals/UK%40123", cancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("/v1/cattle/UK%40123", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task Get_sends_api_key_header_to_cattle_api_when_configured()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson(FullyPopulatedCattle()));
        await using var factory = CreateFactory(handler, ApiKey);
        using var client = factory.CreateClient();
        var configuredKey = factory.Services.GetRequiredService<IConfiguration>()["CattleApi:ApiKey"];

        await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.False(string.IsNullOrWhiteSpace(configuredKey));
        Assert.Equal(configuredKey, Assert.Single(request.Headers.GetValues("x-api-key")));
    }

    [Fact]
    public async Task Get_returns_200_with_null_optional_sections_for_minimal_cattle_api_response()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson(new CattleDetailsResponse
        {
            EarTag = KnownIdentifier,
            Species = "Cattle",
            Sex = "Male",
            DateRegistered = new DateOnly(2024, 3, 15)
        }));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.NotNull(animal);
        Assert.Equal(KnownIdentifier, animal.AnimalDetails.Identifier);
        Assert.Equal(new DateOnly(2024, 3, 15), animal.Registration.Date);
        Assert.Null(animal.Breed);
        Assert.Null(animal.CurrentSite);
        Assert.Null(animal.Death);
        Assert.Null(animal.ImportParents);
        Assert.Null(animal.Registration.Sire);
        Assert.Null(animal.Registration.GeneticDam);
        Assert.Null(animal.Registration.BirthDam);
    }

    [Fact]
    public async Task Get_returns_404_lis_error_for_unknown_identifier()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/UK00000000000", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found",
            "The requested resource was not found", "section-15.5.5", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_404_lis_error_for_blank_identifier_without_calling_cattle_api()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/%20", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found",
            "The requested resource was not found", "section-15.5.5", cancellationToken);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Get_returns_404_lis_error_when_cattle_api_200_body_is_null()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => RawJson("null"));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found",
            "The requested resource was not found", "section-15.5.5", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_cattle_api_200_body_is_malformed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => RawJson("{ not json"));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    [Theory]
    [InlineData("\"species\":\"unknown\",\"sex\":\"Female\",\"dateRegistered\":\"2024-03-15\"")]
    [InlineData("\"species\":\"Cattle\",\"sex\":\"unknown\",\"dateRegistered\":\"2024-03-15\"")]
    [InlineData("\"species\":\"Cattle\",\"sex\":\"Female\",\"dateRegistered\":null")]
    public async Task Get_returns_500_lis_error_when_cattle_api_required_field_is_unsupported_or_missing(
        string fields)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => RawJson(
            $$"""{"earTag":"{{KnownIdentifier}}",{{fields}}}"""));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, "Forbidden", "The resource could not be returned as the requestor is not authorized", "section-15.5.4")]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError, "Server Error", "The server encountered an unexpected error", "section-15.6.1")]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, "Server Error", "The server encountered an unexpected error", "section-15.6.1")]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, "Server Error", "The server encountered an unexpected error", "section-15.6.1")]
    [InlineData(HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The service is currently unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The service is currently unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.GatewayTimeout, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The service is currently unavailable", "section-15.6.4")]
    public async Task Get_maps_cattle_api_failure_status_to_lis_error(
        HttpStatusCode upstreamStatus,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedDetail,
        string expectedTypeSuffix)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(upstreamStatus));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(expectedStatus, response.StatusCode);
        await AssertLisError(response, expectedStatus, expectedTitle, expectedDetail, expectedTypeSuffix, cancellationToken);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_cattle_api_is_unreachable()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => throw new HttpRequestException("connection refused"));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_503_lis_error_when_cattle_api_request_times_out()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => throw new TaskCanceledException("timeout", new TimeoutException()));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.ServiceUnavailable, "Service Unavailable",
            "The service is currently unavailable", "section-15.6.4", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_cattle_api_throws_unexpected_exception()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => throw new InvalidOperationException("boom"));
        await using var factory = CreateFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    private static async Task AssertLisError(
        HttpResponseMessage response,
        HttpStatusCode status,
        string title,
        string detail,
        string typeSuffix,
        CancellationToken cancellationToken)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var errors = await response.Content.ReadFromJsonAsync<LisErrors>(cancellationToken);
        var error = Assert.Single(Assert.IsType<LisErrors>(errors));
        Assert.Equal((int)status, error.Status);
        Assert.Equal(title, error.Title);
        Assert.Equal(detail, error.Detail);
        Assert.EndsWith(typeSuffix, error.Type);
    }

    private static CattleDetailsResponse FullyPopulatedCattle() => new()
    {
        EarTag = KnownIdentifier,
        Species = "Cattle",
        Sex = "Female",
        State = "Registered",
        DateRegistered = new DateOnly(2024, 3, 15),
        BreedCode = "AA",
        BreedName = "Aberdeen Angus",
        CurrentLocation = new CattleLocationResponse { Identifier = "11/111/1111", Name = "Current Farm", PremisesType = "AH" },
        RegisteredLocation = new CattleLocationResponse { Identifier = "22/222/2222", Name = "Birth Farm", PremisesType = "AH" },
        DeathLocation = new CattleLocationResponse { Identifier = "33/333/3333", Name = "Abattoir", PremisesType = "SL" },
        SireEarTag = "UK000000000001",
        SireName = "Big Bull",
        GeneticDamEarTag = "UK000000000002",
        SurrogateDamEarTag = "UK000000000003"
    };

    private static HttpResponseMessage OkJson(CattleDetailsResponse body) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(body) };

    private static HttpResponseMessage RawJson(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static WebApplicationFactory<Program> CreateFactory(
        StubCattleApiHandler handler,
        string? apiKey = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("CattleApi:BaseUrl", CattleApiBaseUrl);
            builder.UseSetting("CattleApi:ClientModes:GetAnimalByIdentifier", CattleApiClientKeys.Real);
            if (apiKey is not null)
            {
                builder.UseSetting("CattleApi:ApiKey", apiKey);
            }

            builder.ConfigureServices(services =>
                services.AddHttpClient<CattleApiClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler));
        });

    private sealed class StubCattleApiHandler(
        Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(respond(request));
        }
    }
}
