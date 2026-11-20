using System.Diagnostics.CodeAnalysis;
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

        if (animal.Registration.Site is null)
        {
            error = "registration.site is required";
            return false;
        }

        if (animal.Registration.Site.Identifiers is null)
        {
            error = "registration.site.identifiers is required";
            return false;
        }

        if (animal.Registration.Site.Identifiers.Any(i => i is null))
        {
            error = "registration.site.identifiers must not contain null entries";
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

        var cph = animal.Registration.Site.Identifiers.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Identifier))?.Identifier;
        if (cph is null)
        {
            error = "registration.site identifier is required";
            return false;
        }

        var segments = cph.Split('/');
        if (segments.Length != 3 || segments.Any(s => string.IsNullOrWhiteSpace(s) || s is "." or ".."))
        {
            error = "registration.site identifier must be in county/parish/holding format";
            return false;
        }

        if (animal.Birth?.Date is { } birthDate && birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
        {
            error = "birth.date cannot be in the future";
            return false;
        }

        request = animal;
        return true;
    }
}
