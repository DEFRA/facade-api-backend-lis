using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using LisFacadeApiBackend.Facade.Endpoints.Validators;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators;

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
            .Produces<LisErrors>(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/{identifier}", GetAnimalByIdentifier)
            .WithName("GetByIdentifier")
            .Produces<Animal>(StatusCodes.Status200OK)
            .Produces<LisErrors>(StatusCodes.Status403Forbidden)
            .Produces<LisErrors>(StatusCodes.Status404NotFound)
            .Produces<LisErrors>(StatusCodes.Status500InternalServerError)
            .Produces<LisErrors>(StatusCodes.Status503ServiceUnavailable);

        return group;
    }

    private sealed class EndpointLogCategory;

    private static async Task<IResult> GetAnimals(
        [FromQuery] string? species,
        [FromQuery] string? siteIdentifier,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
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
}