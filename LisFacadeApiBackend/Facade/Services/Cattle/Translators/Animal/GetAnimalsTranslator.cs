using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;

namespace LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;

public class GetAnimalsTranslator(
    GetAnimalsService animalsService,
    ILogger<GetAnimalsTranslator> logger)
    : ITranslator<GetAnimalsRequest, GetAnimalsResponse?>
{
    public async Task<GetAnimalsResponse?> Translate(GetAnimalsRequest inboundRequest, CancellationToken cancellationToken)
    {
        if (inboundRequest.Species != Species.Cattle)
        {
            return new GetAnimalsResponse { Count = 0, Values = [] };
        }

        var cattle = await animalsService.GetAnimalsAsync(inboundRequest.SiteIdentifier, cancellationToken);
        if (cattle is null)
        {
            logger.LogDebug("No site found for identifier {SiteIdentifier}", inboundRequest.SiteIdentifier);
            return null;
        }

        var summaries = new List<AnimalSummary>();
        foreach (var animal in cattle)
        {
            var summary = ToSummary(animal);
            if (summary is null)
            {
                logger.LogWarning("Skipping animal {EarTag} with missing or unsupported sex", animal.EarTag);
                continue;
            }

            summaries.Add(summary);
        }

        Func<AnimalSummary, string?> sortKey = inboundRequest.Order == AnimalOrder.State
            ? summary => summary.Animal?.State?.ToString()
            : summary => summary.Animal?.Identifier;
        var ordered = string.Equals(inboundRequest.Sort, "desc", StringComparison.OrdinalIgnoreCase)
            ? summaries.OrderByDescending(sortKey, StringComparer.OrdinalIgnoreCase)
            : summaries.OrderBy(sortKey, StringComparer.OrdinalIgnoreCase);

        var total = summaries.Count;
        var pageSize = inboundRequest.PageSize ?? total;
        var skip = (long)(inboundRequest.Page - 1) * pageSize;
        if (skip > 0 && skip >= total)
        {
            logger.LogInformation(
                "Page {Page} with page size {PageSize} is beyond the {Count} animals for site {SiteIdentifier}, returning no values",
                inboundRequest.Page, pageSize, total, inboundRequest.SiteIdentifier);
        }

        var values = skip >= total
            ? []
            : ordered.Skip((int)skip).Take(pageSize).ToList();

        return new GetAnimalsResponse { Count = total, Values = values };
    }

    private static AnimalSummary? ToSummary(CattleResponse cattle)
    {
        var sex = EnumParser.ParseOrNull<Sex>(cattle.Sex);
        if (sex is null)
        {
            return null;
        }

        return new AnimalSummary
        {
            Animal = new AnimalSummaryAnimal
            {
                Identifier = cattle.EarTag,
                Species = Species.Cattle,
                Sex = sex.Value,
                State = EnumParser.ParseOrNull<AnimalState>(cattle.Status)
            },
            Breed = cattle.BreedCode is null && cattle.BreedName is null
                ? null
                : new Breed
                {
                    Code = cattle.BreedCode,
                    Name = cattle.BreedName
                }
        };
    }
}
