using Microsoft.Extensions.DependencyInjection;

namespace LiteApi;

public static class LiteApiServerExtensions
{
    public static IServiceCollection AddLiteApi(this IServiceCollection services, int port)
    {
        services.AddHostedService(services => ActivatorUtilities.CreateInstance<LiteApiServer>(services, port));
        return services;
    }
}
