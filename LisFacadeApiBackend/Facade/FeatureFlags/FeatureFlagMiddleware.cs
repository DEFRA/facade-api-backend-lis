using LisFacadeApiBackend.Facade.Endpoints;
using Microsoft.FeatureManagement;

namespace LisFacadeApiBackend.Facade.FeatureFlags;

public sealed class FeatureFlagMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IFeatureManager featureManager)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<FeatureFlagMetadata>();

        if (metadata is not null && !await featureManager.IsEnabledAsync(metadata.FeatureName))
        {
            await LisErrorResults.NotFound().ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}
