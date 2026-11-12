using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.IntegrationTests.GetAnimals;

public class GetAnimalsIntegrationTests
{
    private const string SiteIdentifier = "28/822/2977";
    private const string ListPath = "/animals";
    private const string CattleApiBaseUrl = "http://cattle-api.test/";
    private const string ApiKey = "test-api-key";
    private const string HoldingPath = "/v1/holdings/28/822/2977/cattle";
    private const string RequestIdHeader = "x-cdp-request-id";

    [Fact]
    public async Task Get_returns_200_with_count_and_mapped_values_for_site_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(
            Cattle("UK000000000002", "female", "Registered", "AA", "Aberdeen Angus"),
            Cattle("UK000000000001", "male", "Dead", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Equal(2, body.Values!.Count);

        var first = body.Values[0];
        Assert.Equal("UK000000000001", first.Animal!.Identifier);
        Assert.Equal(Species.Cattle, first.Animal.Species);
        Assert.Equal(Sex.Male, first.Animal.Sex);
        Assert.Equal(AnimalState.Dead, first.Animal.State);
        Assert.Null(first.Breed);

        var second = body.Values[1];
        Assert.Equal("UK000000000002", second.Animal!.Identifier);
        Assert.Equal(Sex.Female, second.Animal.Sex);
        Assert.Equal(AnimalState.Registered, second.Animal.State);
        Assert.Equal("AA", second.Breed?.Code);
        Assert.Equal("Aberdeen Angus", second.Breed?.Name);
    }

    [Fact]
    public async Task Get_passes_site_identifier_to_cattle_api_client_without_filters_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson());
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await upstream.Received(1).GetCattleForHoldingMultiSegmentAsync("28", "822", "2977", null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_returns_200_with_empty_list_when_site_has_no_animals_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson());
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(0, body.Count);
        Assert.Empty(body.Values!);
    }

    [Theory]
    [InlineData("sheep")]
    [InlineData("goats")]
    [InlineData("deer")]
    public async Task Get_returns_empty_list_for_non_cattle_species_without_calling_upstream(string species)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(species), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(0, body.Count);
        Assert.Empty(body.Values!);
        await upstream.DidNotReceive().GetCattleForHoldingMultiSegmentAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("/animals?siteIdentifier=28%2F822%2F2977")]
    [InlineData("/animals?species=&siteIdentifier=28%2F822%2F2977")]
    [InlineData("/animals?species=pigs&siteIdentifier=28%2F822%2F2977")]
    [InlineData("/animals?species=cattle")]
    [InlineData("/animals?species=cattle&siteIdentifier=%20")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&page=0")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&pageSize=0")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&page=abc")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&pageSize=abc")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&page=2147483648")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&pageSize=2147483648")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&order=ear")]
    [InlineData("/animals?species=cattle&siteIdentifier=28%2F822%2F2977&sort=up")]
    public async Task Get_returns_404_lis_error_for_invalid_query_without_calling_upstream(string url)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url, cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found", "section-15.5.5", cancellationToken);
        await upstream.DidNotReceive().GetCattleForHoldingMultiSegmentAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Open_api_document_declares_page_and_page_size_as_integers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = FactoryWithUpstream(Substitute.For<ICattleApiClient>());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", cancellationToken);

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var parameters = document.RootElement
            .GetProperty("paths").GetProperty("/animals").GetProperty("get").GetProperty("parameters");
        foreach (var name in new[] { "page", "pageSize" })
        {
            var parameter = parameters.EnumerateArray().Single(p => p.GetProperty("name").GetString() == name);
            Assert.Equal("integer", parameter.GetProperty("schema").GetProperty("type").GetString());
        }
    }

    [Fact]
    public async Task Open_api_document_declares_species_and_site_identifier_as_required()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = FactoryWithUpstream(Substitute.For<ICattleApiClient>());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", cancellationToken);

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var parameters = document.RootElement
            .GetProperty("paths").GetProperty("/animals").GetProperty("get").GetProperty("parameters");
        foreach (var name in new[] { "species", "siteIdentifier" })
        {
            var parameter = parameters.EnumerateArray().Single(p => p.GetProperty("name").GetString() == name);
            Assert.True(parameter.GetProperty("required").GetBoolean());
        }
    }

    [Fact]
    public async Task Get_applies_paging_after_filtering_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(
            Cattle("UK000000000003", "female", "Registered", null, null),
            Cattle("UK000000000001", "male", "Registered", null, null),
            Cattle("UK000000000002", "female", "Registered", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(page: 2, pageSize: 2), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(3, body.Count);
        var value = Assert.Single(body.Values!);
        Assert.Equal("UK000000000003", value.Animal!.Identifier);
    }

    [Fact]
    public async Task Get_returns_empty_values_when_page_is_beyond_the_end_but_keeps_count_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(Cattle("UK000000000001", "male", "Registered", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(page: 5, pageSize: 10), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(1, body.Count);
        Assert.Empty(body.Values!);
    }

    [Fact]
    public async Task Get_orders_by_animal_identifier_ascending_by_default_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(
            Cattle("UK000000000003", "female", "Registered", null, null),
            Cattle("UK000000000001", "male", "Registered", null, null),
            Cattle("UK000000000002", "female", "Registered", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(
            ["UK000000000001", "UK000000000002", "UK000000000003"],
            body.Values!.Select(v => v.Animal!.Identifier));
    }

    [Fact]
    public async Task Get_orders_by_identifier_descending_when_sort_is_desc_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(
            Cattle("UK000000000001", "male", "Registered", null, null),
            Cattle("UK000000000003", "female", "Registered", null, null),
            Cattle("UK000000000002", "female", "Registered", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(sort: "desc"), cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(
            ["UK000000000003", "UK000000000002", "UK000000000001"],
            body.Values!.Select(v => v.Animal!.Identifier));
    }

    [Fact]
    public async Task Get_orders_by_state_when_order_is_state_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(
            Cattle("UK000000000001", "male", "Lost", null, null),
            Cattle("UK000000000002", "female", "Dead", null, null),
            Cattle("UK000000000003", "female", "Registered", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(order: "state"), cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(
            ["UK000000000002", "UK000000000001", "UK000000000003"],
            body.Values!.Select(v => v.Animal!.Identifier));
    }

    [Fact]
    public async Task Get_returns_404_lis_error_when_upstream_site_is_not_found_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(new HttpResponseMessage(HttpStatusCode.NotFound));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found", "section-15.5.5", cancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, "Forbidden", "section-15.5.4")]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1")]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1")]
    public async Task Get_maps_upstream_failure_to_lis_error_fake_client(
        HttpStatusCode upstreamStatus,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedTypeSuffix)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(new HttpResponseMessage(upstreamStatus));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(expectedStatus, response.StatusCode);
        await AssertLisError(response, expectedStatus, expectedTitle, expectedTypeSuffix, cancellationToken);
    }

    [Fact]
    public async Task Get_skips_animal_with_missing_sex_and_returns_the_rest_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(OkJson(
            Cattle("UK000000000001", null, "Registered", null, null),
            Cattle("UK000000000002", "female", "Registered", null, null)));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(1, body.Count);
        Assert.Equal("UK000000000002", Assert.Single(body.Values!).Animal!.Identifier);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_upstream_throws_unexpected_exception_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.GetCattleForHoldingMultiSegmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(_ => throw new InvalidOperationException("boom"));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1", cancellationToken);
    }

    [Fact]
    public async Task Get_calls_cattle_api_holding_endpoint_with_empty_filters_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson());
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(HoldingPath, request.RequestUri!.AbsolutePath);
        Assert.Equal("?earTag=&breed=&sex=", request.RequestUri.Query);
    }

    [Fact]
    public async Task Get_sends_api_key_header_to_cattle_api_when_configured_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson());
        await using var factory = CreateRealClientFactory(handler, ApiKey);
        using var client = factory.CreateClient();
        var configuredKey = factory.Services.GetRequiredService<IConfiguration>()["CattleApi:ApiKey"];

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var request = Assert.Single(handler.Requests);
        Assert.False(string.IsNullOrWhiteSpace(configuredKey));
        Assert.Equal(configuredKey, Assert.Single(request.Headers.GetValues("x-api-key")));
    }

    [Fact]
    public async Task Get_returns_200_with_mapped_animals_from_cattle_api_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson(
            Cattle("UK000000000002", "female", "Registered", "AA", "Aberdeen Angus"),
            Cattle("UK000000000001", "male", "Dead", null, null)));
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Equal(["UK000000000001", "UK000000000002"], body.Values!.Select(v => v.Animal!.Identifier));

        var first = body.Values![0];
        Assert.Equal(Species.Cattle, first.Animal!.Species);
        Assert.Equal(Sex.Male, first.Animal.Sex);
        Assert.Equal(AnimalState.Dead, first.Animal.State);
        Assert.Null(first.Breed);

        var second = body.Values[1];
        Assert.Equal(Sex.Female, second.Animal!.Sex);
        Assert.Equal(AnimalState.Registered, second.Animal.State);
        Assert.Equal("AA", second.Breed?.Code);
        Assert.Equal("Aberdeen Angus", second.Breed?.Name);
    }

    [Fact]
    public async Task Get_returns_200_with_empty_list_when_cattle_api_returns_no_animals_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson());
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(0, body.Count);
        Assert.Empty(body.Values!);
    }

    [Fact]
    public async Task Get_applies_sorting_and_paging_to_cattle_api_results_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson(
            Cattle("UK000000000001", "male", "Registered", null, null),
            Cattle("UK000000000003", "female", "Registered", null, null),
            Cattle("UK000000000002", "female", "Registered", null, null)));
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(page: 1, pageSize: 2, sort: "desc"), cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(3, body.Count);
        Assert.Equal(["UK000000000003", "UK000000000002"], body.Values!.Select(v => v.Animal!.Identifier));
    }

    [Fact]
    public async Task Get_returns_404_lis_error_when_cattle_api_holding_not_found_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found", "section-15.5.5", cancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, "Forbidden", "section-15.5.4")]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.GatewayTimeout, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1")]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1")]
    public async Task Get_maps_cattle_api_error_status_to_lis_error_real_client(
        HttpStatusCode upstreamStatus,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedTypeSuffix)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(upstreamStatus));
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(expectedStatus, response.StatusCode);
        await AssertLisError(response, expectedStatus, expectedTitle, expectedTypeSuffix, cancellationToken);
    }

    [Fact]
    public async Task Get_maps_cattle_api_http_client_timeout_to_service_unavailable_lis_error_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => throw new TaskCanceledException("HttpClient timed out"));
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4", cancellationToken);
    }

    [Fact]
    public async Task Get_maps_cattle_api_connection_failure_to_server_error_lis_error_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => throw new HttpRequestException("connection refused"));
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1", cancellationToken);
    }

    [Fact]
    public async Task Get_returns_500_lis_error_when_cattle_api_body_is_not_valid_json_real_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json", Encoding.UTF8, "application/json")
        });
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(), cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1", cancellationToken);
    }

    [Theory]
    [InlineData("sheep")]
    [InlineData("goats")]
    [InlineData("deer")]
    public async Task Get_returns_empty_list_for_non_cattle_species_without_calling_cattle_api_client(string species)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson());
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(species), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetAnimalsResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal(0, body.Count);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("28/822")]
    [InlineData("28//2977")]
    [InlineData("28/../2977")]
    [InlineData("28/822/.")]
    [InlineData("../822/2977")]
    public async Task Get_returns_404_lis_error_for_malformed_site_identifier_without_calling_cattle_api_client(string siteIdentifier)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new StubCattleApiHandler(_ => OkJson());
        await using var factory = CreateRealClientFactory(handler);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ListUrl(siteIdentifier: siteIdentifier), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.NotFound, "Not found", "section-15.5.5", cancellationToken);
        Assert.Empty(handler.Requests);
    }

    private static string ListUrl(
        string species = "cattle",
        string siteIdentifier = SiteIdentifier,
        int? page = null,
        int? pageSize = null,
        string? order = null,
        string? sort = null)
    {
        var query = $"species={species}&siteIdentifier={Uri.EscapeDataString(siteIdentifier)}";
        if (page is not null) query += $"&page={page}";
        if (pageSize is not null) query += $"&pageSize={pageSize}";
        if (order is not null) query += $"&order={order}";
        if (sort is not null) query += $"&sort={sort}";
        return $"{ListPath}?{query}";
    }

    private static CattleResponse Cattle(string earTag, string? sex, string status, string? breedCode, string? breedName) =>
        new()
        {
            EarTag = earTag,
            Sex = sex,
            Status = status,
            BreedCode = breedCode,
            BreedName = breedName
        };

    private static async Task AssertLisError(
        HttpResponseMessage response,
        HttpStatusCode status,
        string title,
        string typeSuffix,
        CancellationToken cancellationToken)
    {
        var errors = await response.Content.ReadFromJsonAsync<LisErrors>(cancellationToken);
        var error = Assert.Single(Assert.IsType<LisErrors>(errors));
        Assert.Equal((int)status, error.Status);
        Assert.Equal(title, error.Title);
        Assert.EndsWith(typeSuffix, error.Type);
    }

    private static ICattleApiClient UpstreamReturning(HttpResponseMessage response)
    {
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.GetCattleForHoldingMultiSegmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(response));
        return upstream;
    }

    private static HttpResponseMessage OkJson(params CattleResponse[] cattle) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(cattle) };

    private static WebApplicationFactory<Program> FactoryWithUpstream(ICattleApiClient upstream) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddKeyedSingleton(CattleApiClientKeys.Fake, upstream)));

    private static WebApplicationFactory<Program> CreateRealClientFactory(
        StubCattleApiHandler handler,
        string? apiKey = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("CattleApi:BaseUrl", CattleApiBaseUrl);
            builder.UseSetting("CattleApi:ClientModes:GetAnimals", CattleApiClientKeys.Real);
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

            if (!HasNonBlankRequestId(request))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }

            return Task.FromResult(respond(request));
        }

        private static bool HasNonBlankRequestId(HttpRequestMessage request) =>
            request.Headers.TryGetValues(RequestIdHeader, out var values)
            && values.Any(value => !string.IsNullOrWhiteSpace(value));
    }
}
