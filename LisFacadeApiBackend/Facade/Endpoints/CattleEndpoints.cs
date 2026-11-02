using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Error;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators;

namespace LisFacadeApiBackend.Facade.Endpoints;

[ExcludeFromCodeCoverage]
public static class CattleEndpoints
{
    public static RouteGroupBuilder MapCattleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/animals")
            .WithTags("Animals");

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