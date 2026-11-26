using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;

public class PostAnimalsService(
    ICattleApiClient cattleApi,
    ILogger<PostAnimalsService> logger)
{
    public async Task<BundleResponse> CreateBundleAsync(RegistrationBundleRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Running {implementation} for POST registration bundle", cattleApi.GetType());

        using var response = await cattleApi.CreateRegistrationBundleAsync(request, cancellationToken);
        EnsureSuccess(response, "create registration bundle");

        var bundle = await response.Content.ReadFromJsonAsync<BundleResponse>(cancellationToken);
        return bundle ?? throw new InvalidOperationException("Cattle API returned an empty registration bundle.");
    }

    private void EnsureSuccess(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var level = (int)response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        logger.Log(level, "Cattle API returned {StatusCode} when attempting to {Operation}", (int)response.StatusCode, operation);
        response.EnsureSuccessStatusCode();
    }
}
