using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators;

namespace LisFacadeApiBackend.Facade.Endpoints.Validators;

public static class GetAnimalsRequestValidator
{
    public static bool TryBuild(
        string? species,
        string? siteIdentifier,
        string? page,
        string? pageSize,
        string? order,
        string? sort,
        [NotNullWhen(true)] out GetAnimalsRequest? request,
        [NotNullWhen(false)] out string? error)
    {
        request = null;
        error = null;

        var parsedSpecies = EnumParser.ParseOrNull<Species>(species);
        if (parsedSpecies is null)
        {
            error = "species is required and must be a supported species";
            return false;
        }

        if (string.IsNullOrWhiteSpace(siteIdentifier))
        {
            error = "siteIdentifier is required";
            return false;
        }

        var resolvedPage = 1;
        if (!string.IsNullOrWhiteSpace(page))
        {
            if (!TryParseInt(page, out resolvedPage))
            {
                error = "page must be a whole number";
                return false;
            }

            if (resolvedPage < 1)
            {
                error = "page must be greater than or equal to 1";
                return false;
            }
        }

        int? resolvedPageSize = null;
        if (!string.IsNullOrWhiteSpace(pageSize))
        {
            if (!TryParseInt(pageSize, out var parsedPageSize))
            {
                error = "pageSize must be a whole number";
                return false;
            }

            if (parsedPageSize < 1)
            {
                error = "pageSize must be greater than or equal to 1";
                return false;
            }

            resolvedPageSize = parsedPageSize;
        }

        var resolvedOrder = AnimalOrder.AnimalIdentifier;
        if (!string.IsNullOrWhiteSpace(order))
        {
            var parsedOrder = EnumParser.ParseOrNull<AnimalOrder>(order);
            if (parsedOrder is null)
            {
                error = "order must be 'animalIdentifier' or 'state'";
                return false;
            }

            resolvedOrder = parsedOrder.Value;
        }

        var resolvedSort = "asc";
        if (!string.IsNullOrWhiteSpace(sort))
        {
            if (string.Equals(sort, "desc", StringComparison.OrdinalIgnoreCase))
            {
                resolvedSort = "desc";
            }
            else if (!string.Equals(sort, "asc", StringComparison.OrdinalIgnoreCase))
            {
                error = "sort must be 'asc' or 'desc'";
                return false;
            }
        }

        request = new GetAnimalsRequest
        {
            Species = parsedSpecies.Value,
            SiteIdentifier = siteIdentifier,
            Page = resolvedPage,
            PageSize = resolvedPageSize,
            Order = resolvedOrder,
            Sort = resolvedSort
        };
        return true;
    }

    private static bool TryParseInt(string value, out int result) =>
        int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);
}
