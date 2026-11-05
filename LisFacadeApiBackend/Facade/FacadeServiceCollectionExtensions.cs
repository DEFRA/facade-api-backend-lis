using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;

namespace LisFacadeApiBackend.Facade;

public static class FacadeServiceCollectionExtensions
{
    public static IServiceCollection AddFacadeServices(this IServiceCollection services)
    {
        services.AddTranslators();
        services.AddServices();

        return services;
    }

    public static IServiceCollection AddTranslators(this IServiceCollection services)
    {
        services.AddTransient<ITranslator<string, Animal?>, GetAnimalByIdentifierTranslator>();
        services.AddTransient<ITranslator<GetAnimalsRequest, GetAnimalsResponse?>, GetAnimalsTranslator>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<GetAnimalByIdentifierService>();
        services.AddTransient<GetAnimalsService>();

        return services;
    }
}
