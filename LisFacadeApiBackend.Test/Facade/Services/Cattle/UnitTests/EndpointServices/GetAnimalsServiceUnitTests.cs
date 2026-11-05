using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.EndpointServices;

public class GetAnimalsServiceUnitTests
{
    private const string SiteIdentifier = "12/345/6789";
    private const string County = "12";
    private const string Parish = "345";
    private const string Holding = "6789";

    private readonly ICattleApiClient _client = Substitute.For<ICattleApiClient>();
    private readonly GetAnimalsService _service;

    public GetAnimalsServiceUnitTests()
    {
        _service = new GetAnimalsService(_client, NullLogger<GetAnimalsService>.Instance);
    }

    private void SetupResponse(HttpResponseMessage response) =>
        _client.GetCattleForHoldingMultiSegmentAsync(County, Parish, Holding, null, null, null, Arg.Any<CancellationToken>())
            .Returns(response);

    [Fact]
    public async Task Returns_cattle_for_holding_when_api_returns_ok()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<CattleResponse>
            {
                new() { EarTag = "UK1", Sex = "female", Status = "registered" },
                new() { EarTag = "UK2", Sex = "male", Status = "dead" }
            })
        });

        var result = await _service.GetAnimalsAsync(SiteIdentifier, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(["UK1", "UK2"], result.Select(c => c.EarTag));
    }

    [Fact]
    public async Task Returns_empty_list_when_api_returns_ok_with_no_body()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create<List<CattleResponse>?>(null) });

        var result = await _service.GetAnimalsAsync(SiteIdentifier, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Returns_empty_list_when_api_returns_ok_with_empty_array()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new List<CattleResponse>()) });

        var result = await _service.GetAnimalsAsync(SiteIdentifier, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Returns_null_when_api_returns_not_found()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await _service.GetAnimalsAsync(SiteIdentifier, CancellationToken.None);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Throws_http_request_exception_when_api_returns_error(HttpStatusCode statusCode)
    {
        SetupResponse(new HttpResponseMessage(statusCode));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _service.GetAnimalsAsync(SiteIdentifier, CancellationToken.None));

        Assert.Equal(statusCode, ex.StatusCode);
    }

    [Theory]
    [InlineData("12/345")]
    [InlineData("12/345/6789/extra")]
    [InlineData("")]
    [InlineData("12//6789")]
    [InlineData("12/ /6789")]
    public async Task Returns_null_without_calling_api_when_site_identifier_is_malformed(string siteIdentifier)
    {
        var result = await _service.GetAnimalsAsync(siteIdentifier, CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(_client.ReceivedCalls());
    }

    [Fact]
    public async Task Passes_county_parish_and_holding_segments_to_api_without_filters()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new List<CattleResponse>()) });

        await _service.GetAnimalsAsync(SiteIdentifier, CancellationToken.None);

        await _client.Received(1).GetCattleForHoldingMultiSegmentAsync(
            County, Parish, Holding, null, null, null, Arg.Any<CancellationToken>());
    }
}
