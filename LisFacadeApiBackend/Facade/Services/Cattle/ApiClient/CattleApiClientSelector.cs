using Microsoft.Extensions.Options;

namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public static class CattleApiClientSelector
{
    public static ICattleApiClient ResolveCattleApiClient(this IServiceProvider serviceProvider, string endpointName)
    {
        var clientModes = serviceProvider.GetRequiredService<IOptionsMonitor<CattleApiOptions>>().CurrentValue.ClientModes;

        var key = clientModes.TryGetValue(endpointName, out var configuredKey) ? configuredKey : CattleApiClientKeys.Fake;

        return serviceProvider.GetRequiredKeyedService<ICattleApiClient>(key);
    }
}
