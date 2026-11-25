using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.IntegrationTests.PostAnimals;

public class PostAnimalsIntegrationTests
{
    private const string PostPath = "/animals";
    private const string RequestModeHeader = "LI-Request-Mode";
    private const string ValidBody = """
        {
          "animal": { "identifier": "UK12345678901", "species": "cattle", "sex": "female" },
          "breed": { "code": "AA", "name": "Aberdeen Angus" },
          "registration": {
            "date": "2024-03-15",
            "category": "registration",
            "site": { "identifiers": [ { "identifier": "11/222/3333" } ] }
          }
        }
        """;

    public static TheoryData<string> InvalidBodies => new()
    {
        "",
        "{",
        "null",
        Body(o => o.Remove("animal")),
        Body(o => o.Remove("registration")),
        Body(o => o["animal"]!.AsObject()["species"] = "sheep"),
        Body(o => o["animal"]!.AsObject()["identifier"] = " "),
        Body(o => o["animal"]!.AsObject()["sex"] = "unknown"),
        Body(o => o["registration"]!.AsObject()["category"] = "bogus"),
        Body(o => o["registration"]!.AsObject().Remove("date")),
        Body(o => o["registration"]!.AsObject()["site"]!.AsObject()["identifiers"]!.AsArray()[0]!.AsObject()["identifier"] = "11-222-3333"),
        Body(o => o["registration"]!.AsObject()["site"]!.AsObject()["identifiers"]!.AsArray().Add(new JsonObject { ["identifier"] = "22/333/4444" })),
    };

    [Fact]
    public async Task Post_returns_200_with_animal_and_self_link_for_sync_request_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(BundleCreated(Guid.NewGuid()));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, ValidBody, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<AnimalResponse>(cancellationToken);
        Assert.NotNull(body);
        Assert.Equal("UK12345678901", body.Identifier);
        Assert.Equal("/animals/UK12345678901", body.Links?.Self);
        await upstream.Received(1).CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Post_returns_202_with_bundle_status_for_async_request_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var bundleId = Guid.NewGuid();
        var upstream = UpstreamReturning(BundleCreated(bundleId));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, ValidBody, cancellationToken, requestMode: "async");

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal($"/animals/requests/{bundleId}", response.Headers.Location?.ToString());
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(bundleId.ToString(), document.RootElement.GetProperty("id").GetString());
        Assert.Equal($"/animals/requests/{bundleId}", document.RootElement.GetProperty("requestStatusUrl").GetString());
    }

    [Fact]
    public async Task Post_sends_mapped_animal_to_cattle_api_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(BundleCreated(Guid.NewGuid()));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, ValidBody, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await upstream.Received(1).CreateRegistrationBundleAsync(
            Arg.Is<RegistrationBundleRequest>(r => r.Animals.Single().EarTag == "UK12345678901"
                && r.Animals.Single().Sex == "female"
                && r.Animals.Single().Breed == "AA"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(InvalidBodies))]
    public async Task Post_returns_400_lis_error_for_invalid_body_without_calling_upstream(string body)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, body, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.BadRequest, "Bad Request", "section-15.5.1", cancellationToken);
        await upstream.DidNotReceive().CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Post_returns_400_lis_error_when_request_mode_header_is_invalid_without_calling_upstream()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, ValidBody, cancellationToken, requestMode: "later");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.BadRequest, "Bad Request", "section-15.5.1", cancellationToken);
        await upstream.DidNotReceive().CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Post_returns_415_without_calling_upstream_when_content_type_is_not_json()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();
        using var content = new StringContent(ValidBody, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync(PostPath, content, cancellationToken);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        await upstream.DidNotReceive().CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest, "Bad Request", "section-15.5.1")]
    [InlineData(HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.GatewayTimeout, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "section-15.6.4")]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1")]
    public async Task Post_maps_upstream_failure_to_lis_error_fake_client(
        HttpStatusCode upstreamStatus,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedTypeSuffix)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = UpstreamReturning(new HttpResponseMessage(upstreamStatus));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, ValidBody, cancellationToken);

        Assert.Equal(expectedStatus, response.StatusCode);
        await AssertLisError(response, expectedStatus, expectedTitle, expectedTypeSuffix, cancellationToken);
    }

    [Fact]
    public async Task Post_returns_500_lis_error_when_upstream_throws_unexpected_exception_fake_client()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var upstream = Substitute.For<ICattleApiClient>();
        upstream.CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(_ => throw new InvalidOperationException("boom"));
        await using var factory = FactoryWithUpstream(upstream);
        using var client = factory.CreateClient();

        var response = await PostJsonAsync(client, ValidBody, cancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await AssertLisError(response, HttpStatusCode.InternalServerError, "Server Error", "section-15.6.1", cancellationToken);
    }

    [Fact]
    public async Task Post_returns_400_when_content_type_is_missing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var content = new ByteArrayContent(Encoding.UTF8.GetBytes("{}"));

        var response = await client.PostAsync("/animals", content, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_returns_400_when_charset_is_not_supported()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var content = new ByteArrayContent(Encoding.UTF8.GetBytes("{}"));
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=not-an-encoding");

        var response = await client.PostAsync("/animals", content, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string Body(Action<JsonObject> mutate)
    {
        var body = JsonNode.Parse(ValidBody)!.AsObject();
        mutate(body);
        return body.ToJsonString();
    }

    private static async Task<HttpResponseMessage> PostJsonAsync(
        HttpClient client,
        string body,
        CancellationToken cancellationToken,
        string? requestMode = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, PostPath)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        if (requestMode is not null)
        {
            request.Headers.Add(RequestModeHeader, requestMode);
        }

        return await client.SendAsync(request, cancellationToken);
    }

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
        upstream.CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(response));
        return upstream;
    }

    private static HttpResponseMessage BundleCreated(Guid id) =>
        new(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new BundleResponse
            {
                Id = id,
                ClientReference = "client-reference",
                CountyParishHolding = "11/222/3333",
                SubmittedBy = "submitter",
                Status = "Pending",
                CreatedAt = DateTimeOffset.UtcNow
            })
        };

    private static WebApplicationFactory<Program> FactoryWithUpstream(ICattleApiClient upstream) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddKeyedSingleton(CattleApiClientKeys.Fake, upstream)));
}