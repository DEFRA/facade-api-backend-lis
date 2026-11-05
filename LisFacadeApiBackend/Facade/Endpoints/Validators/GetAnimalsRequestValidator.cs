using System.Diagnostics.CodeAnalysis;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators;

namespace LisFacadeApiBackend.Facade.Endpoints.Validators;

public static class GetAnimalsRequestValidator
{
    public static bool TryBuild(
        string? species,
        string? siteIdentifier,
        int? page,
        int? pageSize,
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

        var resolvedPage = page ?? 1;
        if (resolvedPage < 1)
        {
            error = "page must be greater than or equal to 1";
            return false;
        }

        if (pageSize is < 1)
        {
            error = "pageSize must be greater than or equal to 1";
            return false;
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
            PageSize = pageSize,
            Order = resolvedOrder,
            Sort = resolvedSort
        };
        return true;
    }
}
