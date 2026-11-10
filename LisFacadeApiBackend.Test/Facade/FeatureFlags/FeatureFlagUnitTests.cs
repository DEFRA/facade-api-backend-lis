using System.Net;
using System.Text.Json;
using LisFacadeApiBackend.Facade.Endpoints;
using LisFacadeApiBackend.Facade.FeatureFlags;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests;

public class FeatureFlagUnitTests
{
    private readonly ICattleApiClient _real = Substitute.For<ICattleApiClient>();
    private readonly ICattleApiClient _fake = Substitute.For<ICattleApiClient>();

    private IServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddKeyedSingleton(CattleApiClientKeys.Real, _real);
        services.AddKeyedSingleton(CattleApiClientKeys.Fake, _fake);
        services.AddOptions<CattleApiOptions>().Bind(configuration.GetSection(CattleApiOptions.SectionName));

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Returns_real_client_when_endpoint_is_configured_as_real()
    {
        var provider = BuildProvider(new() { ["CattleApi:ClientModes:GetAnimalByIdentifier"] = CattleApiClientKeys.Real });

        Assert.Same(_real, provider.ResolveCattleApiClient("GetAnimalByIdentifier"));
    }

    [Fact]
    public void Returns_fake_client_when_endpoint_is_configured_as_fake()
    {
        var provider = BuildProvider(new() { ["CattleApi:ClientModes:GetAnimals"] = CattleApiClientKeys.Fake });

        Assert.Same(_fake, provider.ResolveCattleApiClient("GetAnimals"));
    }

    [Fact]
    public void Returns_fake_client_when_endpoint_is_not_configured()
    {
        var provider = BuildProvider(new());

        Assert.Same(_fake, provider.ResolveCattleApiClient("GetAnimals"));
    }

    [Fact]
    public void Resolves_each_endpoint_to_its_own_client_mode()
    {
        var provider = BuildProvider(new()
        {
            ["CattleApi:ClientModes:GetAnimals"] = CattleApiClientKeys.Real,
            ["CattleApi:ClientModes:GetAnimalByIdentifier"] = CattleApiClientKeys.Fake
        });

        Assert.Same(_real, provider.ResolveCattleApiClient("GetAnimals"));
        Assert.Same(_fake, provider.ResolveCattleApiClient("GetAnimalByIdentifier"));
    }

    [Fact]
    public void RequireFeatureFlag_adds_metadata_with_the_flag_name()
    {
        var builder = new RecordingConventionBuilder();

        builder.RequireFeatureFlag(FeatureFlagNames.GetAnimals);

        var metadata = builder.Build().Metadata.OfType<FeatureFlagMetadata>().Single();
        Assert.Equal(FeatureFlagNames.GetAnimals, metadata.FeatureName);
    }

    [Fact]
    public void RequireFeatureFlag_returns_the_same_builder_for_chaining()
    {
        var builder = new RecordingConventionBuilder();

        Assert.Same(builder, builder.RequireFeatureFlag(FeatureFlagNames.GetAnimals));
    }

    [Theory]
    [InlineData(FeatureFlagNames.GetAnimals)]
    [InlineData(FeatureFlagNames.GetAnimalByIdentifier)]
    public async Task Middleware_calls_next_when_feature_flag_is_enabled(string flag)
    {
        var featureManager = Substitute.For<IFeatureManager>();
        featureManager.IsEnabledAsync(flag).Returns(true);
        var context = CreateHttpContext(flag);
        var nextCalled = false;
        var middleware = new FeatureFlagMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context, featureManager);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        await featureManager.Received(1).IsEnabledAsync(flag);
    }

    [Theory]
    [InlineData(FeatureFlagNames.GetAnimals)]
    [InlineData(FeatureFlagNames.GetAnimalByIdentifier)]
    public async Task Middleware_returns_404_lis_error_and_skips_next_when_feature_flag_is_disabled(string flag)
    {
        var featureManager = Substitute.For<IFeatureManager>();
        featureManager.IsEnabledAsync(flag).Returns(false);
        var context = CreateHttpContext(flag);
        var nextCalled = false;
        var middleware = new FeatureFlagMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context, featureManager);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var errors = await JsonSerializer.DeserializeAsync<LisErrors>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            TestContext.Current.CancellationToken);
        Assert.NotNull(errors);
        Assert.Equal((int)HttpStatusCode.NotFound, Assert.Single(errors).Status);
    }

    [Fact]
    public async Task Middleware_calls_next_without_checking_flags_when_endpoint_has_no_feature_flag_metadata()
    {
        var featureManager = Substitute.For<IFeatureManager>();
        var context = CreateHttpContext(featureName: null);
        var nextCalled = false;
        var middleware = new FeatureFlagMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context, featureManager);

        Assert.True(nextCalled);
        await featureManager.DidNotReceive().IsEnabledAsync(Arg.Any<string>());
    }

    private static DefaultHttpContext CreateHttpContext(string? featureName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<JsonOptions>(_ => { });

        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Response.Body = new MemoryStream();

        if (featureName is not null)
        {
            context.SetEndpoint(new Endpoint(
                requestDelegate: null,
                metadata: new EndpointMetadataCollection(new FeatureFlagMetadata(featureName)),
                displayName: featureName));
        }

        return context;
    }

    private sealed class RecordingConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<Action<EndpointBuilder>> _conventions = [];

        public void Add(Action<EndpointBuilder> convention) => _conventions.Add(convention);

        public RouteEndpointBuilder Build()
        {
            var endpoint = new RouteEndpointBuilder(null, RoutePatternFactory.Parse("/"), order: 0);
            foreach (var convention in _conventions)
            {
                convention(endpoint);
            }

            return endpoint;
        }
    }
}
