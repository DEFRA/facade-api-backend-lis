using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using LisFacadeApiBackend.Facade.Endpoints.Validators;
using LisFacadeApiBackend.Facade.FeatureFlags;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;

namespace LisFacadeApiBackend.Facade.Endpoints;

[ExcludeFromCodeCoverage]
public static class CattleEndpoints
{
    public static RouteGroupBuilder MapCattleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/animals")
            .WithTags("Animals");

        group.MapGet("/", GetAnimals)
            .WithName("GetAnimals")
            .Produces<GetAnimalsResponse>(StatusCodes.Status200OK)
            .Produces<LisErrors>(StatusCodes.Status403Forbidden)
            .Produces<LisErrors>(StatusCodes.Status404NotFound)
            .Produces<LisErrors>(StatusCodes.Status500InternalServerError)
            .Produces<LisErrors>(StatusCodes.Status503ServiceUnavailable)
            .AddOpenApiOperationTransformer(SetIntegerPagingParameters)
            .AddOpenApiOperationTransformer(SetRequiredSiteParameters)
            .RequireFeatureFlag(FeatureFlagNames.GetAnimals);

        group.MapGet("/{identifier}", GetAnimalByIdentifier)
            .WithName("GetByIdentifier")
            .Produces<Animal>(StatusCodes.Status200OK)
            .Produces<LisErrors>(StatusCodes.Status403Forbidden)
            .Produces<LisErrors>(StatusCodes.Status404NotFound)
            .Produces<LisErrors>(StatusCodes.Status500InternalServerError)
            .Produces<LisErrors>(StatusCodes.Status503ServiceUnavailable)
            .RequireFeatureFlag(FeatureFlagNames.GetAnimalByIdentifier);

        group.MapPost("/", PostAnimals)
            .WithName("PostAnimals")
            .Produces<AnimalResponse>(StatusCodes.Status200OK)
            .Produces<LisErrors>(StatusCodes.Status400BadRequest)
            .Produces<LisErrors>(StatusCodes.Status500InternalServerError)
            .Produces<LisErrors>(StatusCodes.Status503ServiceUnavailable)
            .Accepts<Animal>("application/json")
            .RequireFeatureFlag(FeatureFlagNames.PostAnimals);
            
        return group;
    }

    private sealed class EndpointLogCategory;

    private static Task SetIntegerPagingParameters(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var parameter in operation.Parameters ?? [])
        {
            if (parameter.In == ParameterLocation.Query && parameter.Name is "page" or "pageSize" && parameter.Schema is OpenApiSchema schema)
            {
                schema.Type = JsonSchemaType.Integer;
                schema.Format = "int32";
            }
        }

        return Task.CompletedTask;
    }

    private static Task SetRequiredSiteParameters(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var parameter in operation.Parameters ?? [])
        {
            if (parameter.In == ParameterLocation.Query && parameter.Name is "species" or "siteIdentifier" && parameter is OpenApiParameter concrete)
            {
                concrete.Required = true;
            }
        }

        return Task.CompletedTask;
    }

    private static async Task<IResult> GetAnimals(
        [FromQuery] string? species,
        [FromQuery] string? siteIdentifier,
        [FromQuery] string? page,
        [FromQuery] string? pageSize,
        [FromQuery] string? order,
        [FromQuery] string? sort,
        [FromServices] ITranslator<GetAnimalsRequest, GetAnimalsResponse?> translator,
        ILogger<EndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("GET /animals received");
        if (!GetAnimalsRequestValidator.TryBuild(species, siteIdentifier, page, pageSize, order, sort, out var request, out var error))
        {
            logger.LogWarning("Invalid GET /animals request: {Error}", error);
            return LisErrorResults.NotFound();
        }

        GetAnimalsResponse? result;
        try
        {
            result = await translator.Translate(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Upstream failure for site {SiteIdentifier}", request.SiteIdentifier);
            return LisErrorResults.FromUpstream(ex.StatusCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error for site {SiteIdentifier}", request.SiteIdentifier);
            return LisErrorResults.ServerError();
        }

        if (result is null)
        {
            logger.LogWarning("Site {SiteIdentifier} not found, returning 404", request.SiteIdentifier);
            return LisErrorResults.NotFound();
        }

        logger.LogInformation("Returning {Count} animals for site {SiteIdentifier}", result.Count, request.SiteIdentifier);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetAnimalByIdentifier(
        [FromRoute] string identifier,
        [FromServices] ITranslator<string, Animal?> translator,
        ILogger<EndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("GET /animals/{Identifier} received", identifier);
        if (string.IsNullOrWhiteSpace(identifier))
        {
            logger.LogWarning("Blank animal identifier received, returning 404");
            return LisErrorResults.NotFound();
        }

        Animal? result;
        try
        {
            result = await translator.Translate(identifier, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Upstream failure for {Identifier}", identifier);
            return LisErrorResults.FromUpstream(ex.StatusCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error for {Identifier}", identifier);
            return LisErrorResults.ServerError();
        }

        if (result is null)
        {
            logger.LogWarning("Animal {Identifier} not found, returning 404", identifier);
            return LisErrorResults.NotFound();
        }

        logger.LogInformation("Returning animal with identifier {Identifier}", identifier);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> PostAnimals(
        HttpRequest httpRequest,
        [FromServices] ITranslator<Animal, AnimalResponse?> translator,
        ILogger<EndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("POST /animals received");

        Animal? animal;
        try
        {
            animal = await httpRequest.ReadFromJsonAsync<Animal>(cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid POST /animals body");
            return LisErrorResults.BadRequest(DescribeJsonError(ex));
        }

        if (animal is null)
        {
            logger.LogWarning("POST /animals body was empty");
            return LisErrorResults.BadRequest("The request body must be an animal");
        }

        if (!PostAnimalsRequestValidator.TryBuild(animal, out var request, out var error))
        {
            logger.LogWarning("Invalid POST /animals request: {Error}", error);
            return LisErrorResults.BadRequest(error);
        }

        AnimalResponse? result;
        try
        {
            result = await translator.Translate(request, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            logger.LogWarning(ex, "Register an Animal rejected animal {Identifier}", request.AnimalDetails.Identifier);
            return LisErrorResults.BadRequest();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Upstream failure for animal {Identifier}", request.AnimalDetails.Identifier);
            return ex.StatusCode is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout
                ? LisErrorResults.ServiceUnavailable()
                : LisErrorResults.ServerError();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error for animal {Identifier}", request.AnimalDetails.Identifier);
            return LisErrorResults.ServerError();
        }

        logger.LogInformation("Registered animal {Identifier}", request.AnimalDetails.Identifier);
        return TypedResults.Ok(result);
    }

    private static string DescribeJsonError(JsonException ex)
    {
        var reason = ex.Message.Split(" Path:")[0].TrimEnd('.');
        return ex.Path is null
            ? $"The request body is invalid: {reason}."
            : $"The request body is invalid at {ex.Path}: {reason}.";
    }
}