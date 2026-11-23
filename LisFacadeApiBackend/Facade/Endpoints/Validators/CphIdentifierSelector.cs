using System.Diagnostics.CodeAnalysis;

namespace LisFacadeApiBackend.Facade.Endpoints.Validators;

public static class CphIdentifierSelector
{
    public static bool TrySelect(
        IEnumerable<string?> identifiers,
        [NotNullWhen(true)] out string? cph,
        [NotNullWhen(false)] out string? error)
    {
        cph = null;
        error = null;

        var matches = identifiers.Where(IsCph).Distinct(StringComparer.Ordinal).ToList();
        if (matches.Count == 0)
        {
            error = "registration.site identifier must be in county/parish/holding format";
            return false;
        }

        if (matches.Count > 1)
        {
            error = "registration.site must contain only one county/parish/holding identifier";
            return false;
        }

        cph = matches[0];
        return true;
    }

    private static bool IsCph(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return false;
        }

        var segments = identifier.Split('/');
        return segments.Length == 3
            && segments.All(s => !string.IsNullOrWhiteSpace(s) && s is not "." and not "..");
    }
}
