using LisFacadeApiBackend.Facade;
using LisFacadeApiBackend.Facade.Endpoints;
using LisFacadeApiBackend.Facade.FeatureFlags;
using LisFacadeApiBackend.Facade.Services;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Config;
using LisFacadeApiBackend.Utils;
using LisFacadeApiBackend.Utils.Http;
using LisFacadeApiBackend.Utils.Mongo;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using LisFacadeApiBackend.Utils.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;
using MongoDB.Driver;
using MongoDB.Driver.Authentication.AWS;
using Serilog;

var app = BuildApp(args);
await app.RunAsync();

[ExcludeFromCodeCoverage]
static WebApplication BuildApp(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureHost(builder);
    ConfigureServices(builder);

    var app = builder.Build();

    WarnIfCattleApiSettingsMissing(app.Configuration, app.Logger);
    ConfigureMiddleware(app);
    ConfigureEndpoints(app);

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureHost(WebApplicationBuilder builder)
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
    }

    builder.Host.UseSerilog(CdpLogging.Configuration);
}

[ExcludeFromCodeCoverage]
static void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    // Trust material must be loaded before anything creates outbound connections.
    services.LoadCustomTrustStoreFromEnvironment();

    services.AddProblemDetails();
    services.AddValidation();
    services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

    services.AddHttpContextAccessor();

    ConfigureHeaderPropagation(services, configuration);
    ConfigureHttpClients(services, configuration);
    ConfigureMongo(services, configuration);

    services.AddHealthChecks();
    services.AddOpenApi();
    services.AddFeatureManagement();

    // App services
    services.AddSingleton<IExamplePersistence, ExamplePersistence>();
    services.AddFacadeServices();
}

[ExcludeFromCodeCoverage]
static void ConfigureHeaderPropagation(IServiceCollection services, IConfiguration configuration)
{
    var traceHeader = configuration.GetValue<string>("TraceHeader");

    services.AddHeaderPropagation(options =>
    {
        if (!string.IsNullOrWhiteSpace(traceHeader))
        {
            options.Headers.Add(traceHeader);
        }
    });
}

[ExcludeFromCodeCoverage]
static void ConfigureHttpClients(IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton<FakeCattleApiClient>();
    services.AddHttpClientWithTracing<CattleApiClient, CattleApiClient>()
        .ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                var baseUrl = config["CattleApi:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException(
                        "CattleApi:BaseUrl is not configured. Set the CattleApi__BaseUrl environment variable.");
                }

                client.BaseAddress = new Uri(baseUrl);

                var apiKey = config["CattleApi:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                }
            });
    
    services.AddKeyedSingleton<ICattleApiClient>(
        CattleApiClientKeys.Fake,
        (sp, _) => sp.GetRequiredService<FakeCattleApiClient>());
    services.AddKeyedTransient<ICattleApiClient>(
        CattleApiClientKeys.Real,
        (sp, _) => sp.GetRequiredService<CattleApiClient>());

    services.AddOptions<CattleApiOptions>()
        .Bind(configuration.GetSection(CattleApiOptions.SectionName));
}

[ExcludeFromCodeCoverage]
static void WarnIfCattleApiSettingsMissing(IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
{
    if (string.IsNullOrWhiteSpace(configuration["CattleApi:BaseUrl"]))
    {
        logger.LogWarning("CattleApi:BaseUrl is not set (CattleApi__BaseUrl). Calls to the cattle API will fail.");
    }

    if (string.IsNullOrWhiteSpace(configuration["CattleApi:ApiKey"]))
    {
        logger.LogWarning("CattleApi:ApiKey is not set (CattleApi__ApiKey). Requests to the cattle API will be sent without an x-api-key header.");
    }
}

[ExcludeFromCodeCoverage]
static void ConfigureMongo(IServiceCollection services, IConfiguration configuration)
{

    MongoExtensions.Register();
    MongoConventions.Register();

    services
        .AddOptions<MongoConfig>()
        .Bind(configuration.GetRequiredSection("Mongo"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
}

[ExcludeFromCodeCoverage]
static void ConfigureMiddleware(WebApplication app)
{
    app.UseSerilogRequestLogging();

    app.UseHeaderPropagation();
    app.UseMiddleware<FeatureFlagMiddleware>();
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplication app)
{
    app.MapHealthChecks("/health", new HealthCheckOptions());
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });

    app.MapExampleEndpoints();
    app.MapCattleEndpoints();
}