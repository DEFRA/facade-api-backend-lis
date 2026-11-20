using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using AnimalModel = LisFacadeApiBackend.Facade.LIS.Animal.Animal;

namespace LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;

public class PostAnimalsTranslator(
    PostAnimalsService animalsService,
    ILogger<PostAnimalsTranslator> logger)
    : ITranslator<AnimalModel, AnimalResponse?>
{
    public async Task<AnimalResponse?> Translate(AnimalModel inboundRequest, CancellationToken cancellationToken)
    {
        var bundleRequest = TranslateRequest(inboundRequest);
        var earTag = inboundRequest.AnimalDetails.Identifier;

        var bundle = await animalsService.CreateBundleAsync(bundleRequest, cancellationToken);
        logger.LogInformation("Created registration bundle {BundleId} for {EarTag}", bundle.Id, earTag);

        return new AnimalResponse
        {
            Identifier = earTag,
            Links = new AnimalLinks { Self = $"/animals/{Uri.EscapeDataString(earTag)}" }
        };
    }

    public RegistrationBundleRequest TranslateRequest(AnimalModel animal)
    {
        var registration = animal.Registration;
        var cph = registration.Site.Identifiers
            .First(i => !string.IsNullOrWhiteSpace(i.Identifier))
            .Identifier;

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

    private static DamRegistrationRequest? ToDam(AnimalModel animal)
    {
        var surrogateDam = animal.ImportParents?.BirthDam?.Identifier;
        var geneticDam = animal.Registration.GeneticDam?.Identifier;

        return surrogateDam is null && geneticDam is null
            ? null
            : new DamRegistrationRequest
            {
                GeneticDamEarTag = geneticDam,
                SurrogateDamEarTag = surrogateDam
            };
    }

    private static SireRegistrationRequest? ToSire(AnimalModel animal)
    {
        var sire = animal.Registration.Sire?.Identifier ?? animal.ImportParents?.Sire?.Identifier;

        return sire is null ? null : new SireRegistrationRequest { EarTag = sire };
    }
}
