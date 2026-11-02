using System.Net;
using System.Net.Http.Json;
using System.Text;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.IntegrationTests.GetAnimalByIdentifier;

public class GetAnimalByIdentifierIntegrationTests(WebApplicationFactory<Program> sharedFactory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string KnownIdentifier = "UK12345678901";

    [Fact]
    public async Task Get_returns_200_with_translated_animal_for_known_identifier()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = sharedFactory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.NotNull(animal);
        Assert.Equal(KnownIdentifier, animal.AnimalDetails.Identifier);
        Assert.Equal("Holstein Friesian", animal.Breed?.Name);
        Assert.Equal("UK12345678700", animal.Registration.Sire?.Identifier);
        Assert.Equal("UK12345678800", animal.Registration.GeneticDam?.Identifier);
        Assert.Equal("Fake Farm", animal.CurrentSite?.Name);
    }

    [Fact]
    public async Task Get_matches_identifier_case_insensitively()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = sharedFactory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier.ToLowerInvariant()}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_returns_404_lis_error_for_unknown_identifier()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = sharedFactory.CreateClient();

        var response = await client.GetAsync("/animals/UK00000000000", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found",
            "The requested resource was not found", "section-15.5.5", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_404_lis_error_for_blank_identifier_without_calling_upstream()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/%20", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found",
            "The requested resource was not found", "section-15.5.5", cancellationToken);
        await upstream.DidNotReceive().GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_passes_url_decoded_identifier_to_cattle_api_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(new HttpResponseMessage(HttpStatusCode.NotFound));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        await client.GetAsync("/animals/UK%40123", cancellationToken);

        await upstream.Received(1).GetCattleDetailsAsync("UK@123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_maps_fully_populated_upstream_response_to_animal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(new CattleDetailsResponse
        {
            EarTag = "UK111111111111",
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
        }));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/UK111111111111", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.NotNull(animal);

        Assert.Equal("UK111111111111", animal.AnimalDetails.Identifier);
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
    public async Task Get_returns_200_with_null_optional_sections_for_minimal_upstream_response()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(new CattleDetailsResponse
        {
            EarTag = "UK222222222222",
            Species = "Cattle",
            Sex = "Male",
            DateRegistered = new DateOnly(2024, 3, 15)
        }));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/UK222222222222", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.NotNull(animal);
        Assert.Equal("UK222222222222", animal.AnimalDetails.Identifier);
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
    public async Task Get_returns_404_lis_error_when_upstream_200_body_is_null()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(RawJson("null"));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found",
            "The requested resource was not found", "section-15.5.5", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_upstream_200_body_is_malformed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(RawJson("{ not json"));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    [Theory]
    [InlineData("\"sex\":\"female\"")]
    [InlineData("\"species\":\"cattle\"")]
    [InlineData("\"species\":null,\"sex\":\"female\"")]
    [InlineData("\"species\":\"cattle\",\"sex\":null")]
    [InlineData("\"species\":\"\",\"sex\":\"female\"")]
    [InlineData("\"species\":\"cattle\",\"sex\":\"\"")]
    [InlineData("\"species\":\"  \",\"sex\":\"female\"")]
    [InlineData("\"species\":\"cattle\",\"sex\":\"  \"")]
    [InlineData("\"species\":\"unknown\",\"sex\":\"female\"")]
    [InlineData("\"species\":\"cattle\",\"sex\":\"unknown\"")]
    public async Task Get_returns_500_lis_error_when_upstream_required_enum_is_missing_or_unsupported(
        string enumProperties)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(RawJson(
            $$"""{"earTag":"{{KnownIdentifier}}","dateRegistered":"2024-03-15",{{enumProperties}}}"""));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(",\"dateRegistered\":null")]
    public async Task Get_returns_500_lis_error_when_upstream_registration_date_is_missing_or_null(
        string dateProperty)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(RawJson(
            $$"""{"earTag":"{{KnownIdentifier}}","species":"cattle","sex":"female"{{dateProperty}}}"""));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error",
            "The server encountered an unexpected error", "section-15.6.1", cancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, "Forbidden", "The resource could not be returned as the requestor is not authorized", "section-15.5.4")]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError, "Server Error", "The server encountered an unexpected error", "section-15.6.1")]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The service is currently unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The service is currently unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, "Server Error", "The server encountered an unexpected error", "section-15.6.1")]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, "Server Error", "The server encountered an unexpected error", "section-15.6.1")]
    public async Task Get_maps_upstream_failure_to_lis_error(
        HttpStatusCode upstreamStatus,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedDetail,
        string expectedTypeSuffix)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(new HttpResponseMessage(upstreamStatus)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(expectedStatus, response.StatusCode);
        await AssertLisError(response, expectedStatus, expectedTitle, expectedDetail, expectedTypeSuffix, cancellationToken);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_upstream_throws_unexpected_exception()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(_ => throw new InvalidOperationException("boom"));
        await using var factory = FactoryWithUpstream(upstream);
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
        var errors = await response.Content.ReadFromJsonAsync<LisErrors>(cancellationToken);
        var error = Assert.Single(Assert.IsType<LisErrors>(errors));
        Assert.Equal((int)status, error.Status);
        Assert.Equal(title, error.Title);
        Assert.Equal(detail, error.Detail);
        Assert.EndsWith(typeSuffix, error.Type);
    }

    [Fact]
    public async Task Get_passes_identifier_through_to_cattle_api_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/UK99999999999", cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await upstream.Received(1).GetCattleDetailsAsync("UK99999999999", Arg.Any<CancellationToken>());
    }

    private static ICattleApiClient UpstreamReturning(HttpResponseMessage response)
    {
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(response));
        return upstream;
    }

    private static HttpResponseMessage OkJson(CattleDetailsResponse body) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(body) };

    private static HttpResponseMessage RawJson(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static WebApplicationFactory<Program> FactoryWithUpstream(ICattleApiClient upstream) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddKeyedSingleton(CattleApiClientKeys.Fake, upstream)));
}
