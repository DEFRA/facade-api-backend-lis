namespace LisFacadeApiBackend.Facade.FeatureFlags;

public static class FeatureFlagEndpointExtensions
{
    public static TBuilder RequireFeatureFlag<TBuilder>(this TBuilder builder, string featureName)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new FeatureFlagMetadata(featureName));
}
