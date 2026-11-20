using System.Diagnostics.CodeAnalysis;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.Endpoints.Validators;

public static class PostAnimalsRequestValidator
{
    public static bool TryBuild(
        Animal animal,
        [NotNullWhen(true)] out Animal? request,
        [NotNullWhen(false)] out string? error)
    {
        request = null;
        error = null;

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

        // UTC+14 is the furthest ahead of UTC, so a birth date up to one UTC day ahead can still be today locally.
        if (animal.Birth?.Date is { } birthDate && birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
        {
            error = "birth.date cannot be in the future";
            return false;
        }

        request = animal;
        return true;
    }
}
