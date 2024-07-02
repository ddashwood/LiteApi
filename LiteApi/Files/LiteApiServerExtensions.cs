using Microsoft.Extensions.DependencyInjection;

namespace LiteApi.Files;

public static class LiteApiServerExtensions
{
    public static MiddlewarePipelineConfiguration AddFileServer(this MiddlewarePipelineConfiguration config, string root = "wwwroot")
    {
        config.Add(services => ActivatorUtilities.CreateInstance<FileServerMiddleware>(services, root));

        return config;
    }
}
