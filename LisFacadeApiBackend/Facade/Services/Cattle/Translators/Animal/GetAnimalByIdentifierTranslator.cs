using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.LIS.Common;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using AnimalModel = LisFacadeApiBackend.Facade.LIS.Animal.Animal;

namespace LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;

public class GetAnimalByIdentifierTranslator(
    GetAnimalByIdentifierService animalService,
    ILogger<GetAnimalByIdentifierTranslator> logger)
    : ITranslator<string, AnimalModel?>
{
    public async Task<AnimalModel?> Translate(string inboundRequest, CancellationToken cancellationToken)
    {
        var cattleIdentifier = TranslateRequest(inboundRequest);
        var cattle = await animalService.GetAnimalByIdentifierAsync(cattleIdentifier, cancellationToken);
        if (cattle is null)
        {
            logger.LogDebug("No cattle found for identifier {CattleIdentifier}", cattleIdentifier);
            return null;
        }

        return TranslateResponse(cattle);
    }

    public string TranslateRequest(string identifier) => identifier;

    public AnimalModel TranslateResponse(CattleDetailsResponse cattle)
    {
        var species = EnumParser.ParseOrNull<Species>(cattle.Species)
            ?? throw new InvalidOperationException(
                "Upstream cattle response has missing or unsupported species.");
        var sex = EnumParser.ParseOrNull<Sex>(cattle.Sex)
            ?? throw new InvalidOperationException(
                "Upstream cattle response has missing or unsupported sex.");
        var registrationDate = cattle.DateRegistered
            ?? throw new InvalidOperationException(
                "Upstream cattle response has missing registration date.");
        var state = EnumParser.ParseOrNull<AnimalState>(cattle.State);

        return new AnimalModel
        {
            AnimalDetails = new AnimalRecordDetails
            {
                Identifier = cattle.EarTag,
                Species = species,
                Sex = sex,
                State = state,
            },
            Breed = cattle.BreedCode is null && cattle.BreedName is null
                ? null
                : new Breed
                {
                    Code = cattle.BreedCode,
                    Name = cattle.BreedName
                },
            CurrentSite = cattle.CurrentLocation is null ? null : ToSite(cattle.CurrentLocation),
            ImportParents = ToImportParents(cattle),
            Death = cattle.DeathLocation is null
                ? null
                : new AnimalDeath
                {
                    Site = ToSite(cattle.DeathLocation)
                },
            Registration = new AnimalRegistration
            {
                BirthDam = ToParent(cattle.SurrogateDamEarTag),
                GeneticDam = ToParent(cattle.GeneticDamEarTag),
                Sire = ToParent(cattle.SireEarTag, cattle.SireName),
                Site = ToSite(cattle.RegisteredLocation),
                Date = registrationDate,
                Category = default
            }
        };
    }

    private static Site ToSite(CattleLocationResponse? location) =>
        new()
        {
            Type = new SiteType
            {
                Type = location?.PremisesType ?? string.Empty,
                Description = string.Empty
            },
            Identifiers = location?.Identifier is { } identifier
                ? [new SiteIdentifier
                {
                    Identifier = identifier,
                    Type = string.Empty
                }]
                : [],
            Name = location?.Name
        };

    private static ImportParents? ToImportParents(CattleDetailsResponse cattle) =>
        cattle.SireEarTag is null && cattle.GeneticDamEarTag is null && cattle.SurrogateDamEarTag is null
            ? null
            : new ImportParents
            {
                Sire = ToParent(cattle.SireEarTag, cattle.SireName),
                GeneticDam = ToParent(cattle.GeneticDamEarTag),
                BirthDam = ToParent(cattle.SurrogateDamEarTag)
            };

    private static ParentAnimal? ToParent(string? identifier, string? name = null) =>
        identifier is null
            ? null
            : new ParentAnimal
            {
                Identifier = identifier,
                Name = name
            };
}
