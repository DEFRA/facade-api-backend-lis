using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Endpoints.Validators;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;

namespace LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;

public sealed record CreateAnimalResult(AnimalResponse Animal, BundleResponse Bundle);

public class PostAnimalsTranslator(
    PostAnimalsService animalsService,
    ILogger<PostAnimalsTranslator> logger)
    : ITranslator<CreateAnimalRequest, CreateAnimalResult>
{
    public async Task<CreateAnimalResult> Translate(CreateAnimalRequest inboundRequest, CancellationToken cancellationToken)
    {
        var bundleRequest = TranslateRequest(inboundRequest);
        var earTag = inboundRequest.AnimalDetails.Identifier;

        var bundle = await animalsService.CreateBundleAsync(bundleRequest, cancellationToken);
        logger.LogInformation("Created registration bundle {BundleId} for {EarTag}", bundle.Id, earTag);

        var animal = new AnimalResponse
        {
            Identifier = earTag,
            Links = new AnimalLinks { Self = $"/animals/{Uri.EscapeDataString(earTag)}" }
        };
        return new CreateAnimalResult(animal, bundle);
    }

    public RegistrationBundleRequest TranslateRequest(CreateAnimalRequest animal)
    {
        var registration = animal.Registration;
        if (!CphIdentifierSelector.TrySelect(registration.Site.Identifiers.Select(i => i.Identifier), out var cph, out var error))
        {
            throw new InvalidOperationException(error);
        }

        return new RegistrationBundleRequest
        {
            Holding = new HoldingRequest { Cph = cph },
            Animals =
            [
                new AnimalRegistrationRequest
                {
                    EarTag = animal.AnimalDetails.Identifier,
                    DateOfBirth = animal.Birth?.Date,
                    Sex = animal.AnimalDetails.Sex.ToString().ToLowerInvariant(),
                    Breed = animal.Breed?.Code,
                    Dam = ToDam(animal),
                    Sire = ToSire(animal)
                }
            ]
        };
    }

    private static DamRegistrationRequest? ToDam(CreateAnimalRequest animal)
    {
        var surrogateDam = animal.ImportParents?.BirthDam?.Identifier ?? animal.Registration.BirthDam?.Identifier;
        var geneticDam = animal.Registration.GeneticDam?.Identifier ?? animal.ImportParents?.GeneticDam?.Identifier;

        return surrogateDam is null && geneticDam is null
            ? null
            : new DamRegistrationRequest
            {
                GeneticDamEarTag = geneticDam,
                SurrogateDamEarTag = surrogateDam
            };
    }

    private static SireRegistrationRequest? ToSire(CreateAnimalRequest animal)
    {
        var sire = animal.Registration.Sire?.Identifier ?? animal.ImportParents?.Sire?.Identifier;

        return sire is null ? null : new SireRegistrationRequest { EarTag = sire };
    }
}
