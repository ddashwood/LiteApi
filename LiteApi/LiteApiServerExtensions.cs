using LiteApi.Files;
using Microsoft.Extensions.DependencyInjection;

namespace LiteApi;

public static class LiteApiServerExtensions
{
    public static IServiceCollection AddLiteApi(this IServiceCollection services, int port, Action<MiddlewarePipelineConfiguration> config)
    {
        services.AddScoped<FileServerMiddleware>();
        services.AddHostedService(services => ActivatorUtilities.CreateInstance<LiteApiServer>(services, port, config));
        return services;
    }
}
