using System.Net;
using System.Net.Http.Json;
using System.Text;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.EndpointServices;

public class PostAnimalsServiceUnitTests
{
    private const string Cph = "11/222/3333";
    private static readonly Guid BundleId = Guid.NewGuid();

    private readonly ICattleApiClient _client = Substitute.For<ICattleApiClient>();
    private readonly PostAnimalsService _service;

    public PostAnimalsServiceUnitTests()
    {
        _service = new PostAnimalsService(_client, NullLogger<PostAnimalsService>.Instance);
    }

    private static RegistrationBundleRequest BundleRequest() => new()
    {
        Holding = new HoldingRequest { Cph = Cph },
        Animals = []
    };

    private void SetupCreate(HttpResponseMessage response) =>
        _client.CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);

    [Fact]
    public async Task CreateBundleAsync_returns_bundle_when_api_returns_created()
    {
        SetupCreate(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new BundleResponse { Id = BundleId })
        });

        var result = await _service.CreateBundleAsync(BundleRequest(), CancellationToken.None);

        Assert.Equal(BundleId, result.Id);
    }

    [Fact]
    public async Task CreateBundleAsync_forwards_request_to_api_client()
    {
        SetupCreate(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new BundleResponse { Id = BundleId })
        });
        var request = BundleRequest();

        await _service.CreateBundleAsync(request, CancellationToken.None);

        await _client.Received(1).CreateRegistrationBundleAsync(request, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task CreateBundleAsync_throws_when_api_returns_non_success_status(HttpStatusCode statusCode)
    {
        SetupCreate(new HttpResponseMessage(statusCode));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => _service.CreateBundleAsync(BundleRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task CreateBundleAsync_throws_when_api_returns_empty_body()
    {
        SetupCreate(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateBundleAsync(BundleRequest(), CancellationToken.None));

        Assert.Contains("empty registration bundle", ex.Message);
    }
}
