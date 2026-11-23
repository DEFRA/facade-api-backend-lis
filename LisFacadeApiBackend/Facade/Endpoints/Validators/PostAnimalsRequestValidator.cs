using System.Diagnostics.CodeAnalysis;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;

namespace LisFacadeApiBackend.Facade.Endpoints.Validators;

public static class PostAnimalsRequestValidator
{
    public static bool TryBuild(
        CreateAnimalRequest animal,
        [NotNullWhen(true)] out CreateAnimalRequest? request,
        [NotNullWhen(false)] out string? error)
    {
        request = null;
        error = null;

        if (animal.AnimalDetails is null)
        {
            error = "animal is required";
            return false;
        }

        if (animal.Registration is null)
        {
            error = "registration is required";
            return false;
        }

        if (!TryValidateSite(animal.Registration.Site, "registration.site", out error))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(animal.AnimalDetails.Identifier))
        {
            error = "animal.identifier is required";
            return false;
        }

        if (animal.AnimalDetails.Species != Species.Cattle)
        {
            error = "animal.species must be cattle";
            return false;
        }

        if (!Enum.IsDefined(animal.AnimalDetails.Sex))
        {
            error = "animal.sex must be a supported sex";
            return false;
        }

        if (!Enum.IsDefined(animal.Registration.Category))
        {
            error = "registration.category must be a supported category";
            return false;
        }

        if (animal.Registration.Date == default)
        {
            error = "registration.date is required";
            return false;
        }

        if (!CphIdentifierSelector.TrySelect(animal.Registration.Site.Identifiers.Select(i => i.Identifier), out _, out error))
        {
            return false;
        }

        if (animal.Birth is not null && !TryValidateSite(animal.Birth.Site, "birth.site", out error))
        {
            return false;
        }

        if (animal.Birth?.Date is { } birthDate && birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
        {
            error = "birth.date cannot be in the future";
            return false;
        }

        var parents = new (string Path, ParentAnimal? Parent)[]
        {
            ("registration.birthDam", animal.Registration.BirthDam),
            ("registration.geneticDam", animal.Registration.GeneticDam),
            ("registration.sire", animal.Registration.Sire),
            ("importParents.birthDam", animal.ImportParents?.BirthDam),
            ("importParents.geneticDam", animal.ImportParents?.GeneticDam),
            ("importParents.sire", animal.ImportParents?.Sire)
        };

        foreach (var (path, parent) in parents)
        {
            if (parent is not null && parent.Identifier is null)
            {
                error = $"{path}.identifier is required";
                return false;
            }
        }

        request = animal;
        return true;
    }

    private static bool TryValidateSite(CreateSiteRequest? site, string path, [NotNullWhen(false)] out string? error)
    {
        error = null;

        if (site is null)
        {
            error = $"{path} is required";
            return false;
        }

        if (site.Identifiers is null)
        {
            error = $"{path}.identifiers is required";
            return false;
        }

        if (site.Identifiers.Any(i => i is null))
        {
            error = $"{path}.identifiers must not contain null entries";
            return false;
        }

        if (site.Identifiers.Any(i => i.Identifier is null))
        {
            error = $"{path}.identifiers must not contain null identifiers";
            return false;
        }

        return true;
    }
}
