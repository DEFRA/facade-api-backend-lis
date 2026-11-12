using System.Net;
using System.Net.Http.Json;
using System.Text;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.FeatureFlags;

public class FeatureFlagIntegrationTests
{
    private const string KnownIdentifier = "UK111111111111";

    [Theory]
    [InlineData("GetAnimals", "GET", "/animals?species=cattle&siteIdentifier=12%2F345%2F6789", null)]
    [InlineData("GetAnimalByIdentifier", "GET", "/animals/UK12345678901", null)]
    public async Task Returns_404_lis_error_when_endpoint_feature_flag_is_disabled(string flag, string method, string path, string? json)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = FactoryWithFlag(flag, enabled: false);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = json is null ? null : new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request, cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errors = await response.Content.ReadFromJsonAsync<LisErrors>(cancellationToken);
        Assert.NotNull(errors);
        Assert.Equal(404, Assert.Single(errors).Status);
    }

    [Theory]
    [InlineData("GetAnimals", "/animals?species=cattle&siteIdentifier=12%2F345%2F6789")]
    [InlineData("GetAnimalByIdentifier", "/animals/UK12345678901")]
    public async Task Returns_200_when_endpoint_feature_flag_is_enabled(string flag, string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = FactoryWithClientMode(flag, CattleApiClientKeys.Fake, Substitute.For<ICattleApiClient>());
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAnimalByIdentifier_calls_real_client_when_client_mode_is_real()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var real = Substitute.For<ICattleApiClient>();
        real.GetCattleDetailsAsync(KnownIdentifier, Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new CattleDetailsResponse
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
                })
            }));
        var fake = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithClientMode("GetAnimalByIdentifier", CattleApiClientKeys.Real, real, fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/animals/{KnownIdentifier}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.Equal(KnownIdentifier, animal!.AnimalDetails.Identifier);
        await real.Received(1).GetCattleDetailsAsync(KnownIdentifier, Arg.Any<CancellationToken>());
        await fake.DidNotReceive().GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAnimalByIdentifier_uses_fake_client_without_calling_real_when_client_mode_is_fake()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var real = Substitute.For<ICattleApiClient>();
        await using var factory = FactoryWithClientMode("GetAnimalByIdentifier", CattleApiClientKeys.Fake, real);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/animals/UK12345678901", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animal = await response.Content.ReadFromJsonAsync<Animal>(cancellationToken);
        Assert.Equal("UK12345678901", animal!.AnimalDetails.Identifier);
        await real.DidNotReceive().GetCattleDetailsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static WebApplicationFactory<Program> FactoryWithClientMode(
        string endpoint,
        string mode,
        ICattleApiClient real,
        ICattleApiClient? fake = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"FeatureManagement:{endpoint}", "true");
            builder.UseSetting($"CattleApi:ClientModes:{endpoint}", mode);
            builder.ConfigureServices(services =>
            {
                services.AddKeyedSingleton(CattleApiClientKeys.Real, real);
                if (fake is not null)
                {
                    services.AddKeyedSingleton(CattleApiClientKeys.Fake, fake);
                }
            });
        });

    private static WebApplicationFactory<Program> FactoryWithFlag(string flag, bool enabled) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting($"FeatureManagement:{flag}", enabled ? "true" : "false"));
}
