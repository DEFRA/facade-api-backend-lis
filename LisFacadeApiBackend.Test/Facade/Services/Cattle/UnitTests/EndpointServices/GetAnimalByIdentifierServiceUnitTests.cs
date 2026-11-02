using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.EndpointServices;

public class GetAnimalByIdentifierServiceUnitTests
{
    private const string Identifier = "UK12345678901";

    private readonly ICattleApiClient _client = Substitute.For<ICattleApiClient>();
    private readonly GetAnimalByIdentifierService _service;

    public GetAnimalByIdentifierServiceUnitTests()
    {
        _service = new GetAnimalByIdentifierService(_client, NullLogger<GetAnimalByIdentifierService>.Instance);
    }

    private void SetupResponse(HttpResponseMessage response) =>
        _client.GetCattleDetailsAsync(Identifier, Arg.Any<CancellationToken>()).Returns(response);

    [Fact]
    public async Task Returns_cattle_details_when_api_returns_ok()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CattleDetailsResponse { EarTag = Identifier, Breed = "Angus" })
        });

        var result = await _service.GetAnimalByIdentifierAsync(Identifier, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(Identifier, result.EarTag);
        Assert.Equal("Angus", result.Breed);
    }

    [Fact]
    public async Task Returns_null_when_api_returns_not_found()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await _service.GetAnimalByIdentifierAsync(Identifier, CancellationToken.None);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Throws_when_api_returns_non_success_status(HttpStatusCode statusCode)
    {
        SetupResponse(new HttpResponseMessage(statusCode));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _service.GetAnimalByIdentifierAsync(Identifier, CancellationToken.None));

        Assert.Equal(statusCode, ex.StatusCode);
    }

    [Fact]
    public async Task Passes_identifier_and_cancellation_token_to_api_client()
    {
        using var cts = new CancellationTokenSource();
        SetupResponse(new HttpResponseMessage(HttpStatusCode.NotFound));

        await _service.GetAnimalByIdentifierAsync(Identifier, cts.Token);

        await _client.Received(1).GetCattleDetailsAsync(Identifier, cts.Token);
    }
}
