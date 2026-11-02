using System.Net;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;

public class GetAnimalByIdentifierService(
    [FromKeyedServices(CattleApiClientKeys.Fake)] ICattleApiClient cattleApi,
    ILogger<GetAnimalByIdentifierService> logger)
 {
     public async Task<CattleDetailsResponse?> GetAnimalByIdentifierAsync(string cattleIdentifier, CancellationToken cancellationToken)
     {
         logger.LogDebug("Calling cattle API for {CattleIdentifier}", cattleIdentifier);

         using var response = await cattleApi.GetCattleDetailsAsync(cattleIdentifier, cancellationToken);
         if (response.StatusCode == HttpStatusCode.NotFound)
         {
             logger.LogDebug("Cattle API returned 404 for {CattleIdentifier}", cattleIdentifier);
             return null;
         }

         if (!response.IsSuccessStatusCode)
         {
             var level = (int)response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
             logger.Log(level, "Cattle API returned {StatusCode} for {CattleIdentifier}", (int)response.StatusCode, cattleIdentifier);
             response.EnsureSuccessStatusCode();
         }

         var result = await response.Content.ReadFromJsonAsync<CattleDetailsResponse>(cancellationToken);
         logger.LogDebug("Cattle API returned details for {CattleIdentifier}", cattleIdentifier);
         return result;
     }
 }