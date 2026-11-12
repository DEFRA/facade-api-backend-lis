using System.Net;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;

public class GetAnimalsService(
    ICattleApiClient cattleApi,
    ILogger<GetAnimalsService> logger)
{
    public async Task<List<CattleResponse>?> GetAnimalsAsync(string siteIdentifier, CancellationToken cancellationToken)
    {
        logger.LogInformation("Running {implementation} for site GetAnimalAsync {SiteIdentifier}", cattleApi.GetType(), siteIdentifier);

        var segments = siteIdentifier.Split('/');
        if (segments.Length != 3 || segments.Any(s => string.IsNullOrWhiteSpace(s) || s is "." or ".."))
        {
            logger.LogDebug("Site identifier {SiteIdentifier} is not in county/parish/holding format", siteIdentifier);
            return null;
        }

        logger.LogDebug("Calling cattle API for site {SiteIdentifier}", siteIdentifier);

        using var response = await cattleApi.GetCattleForHoldingMultiSegmentAsync(
            segments[0], segments[1], segments[2], null, null, null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogDebug("Cattle API returned 404 for site {SiteIdentifier}", siteIdentifier);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var level = (int)response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
            logger.Log(level, "Cattle API returned {StatusCode} for site {SiteIdentifier}", (int)response.StatusCode, siteIdentifier);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<List<CattleResponse>>(cancellationToken);
        return result ?? [];
    }
}
