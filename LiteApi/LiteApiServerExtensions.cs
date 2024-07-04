using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiteApi;

public static class LiteApiServerExtensions
{
    private static IConfigurationBuilder? _configBuilder;

    public static IConfigurationBuilder AddLiteApiConfiguration(this IConfigurationBuilder builder)
    {
        _configBuilder = builder;
        builder.AddJsonFile("appsettings.liteapi.json", optional: false);
        return builder;
    }

    public static IServiceCollection AddLiteApi(this IServiceCollection services, int port, Action<MiddlewarePipelineConfiguration> config)
    {
        if (_configBuilder == null)
        {
            throw new InvalidOperationException("Cannot add Lite API until the Lite API Configuration has been added");
        }

        var appConfig = _configBuilder.Build();

        services.Configure<LiteApiConfiguration>(appConfig.GetSection("LiteApi"));
        services.AddTransient<HttpRequestFactory>();
        services.AddTransient<HttpResponseProcessor>();

        services.AddHostedService(services => ActivatorUtilities.CreateInstance<LiteApiServer>(services, port, config));

        return services;
    }
}
