using LiteApi;
using LiteApi.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddLiteApiConfiguration();
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
    })
    .ConfigureServices(services =>
    {
        services.AddScoped<M1>();
        services.AddScoped<M2>();
        services.AddScoped<M3>();
        
        services.AddLiteApi(80, config =>
        {
            config.Add<M1>();
            config.Add<M2>();
            config.AddFileServer();
            config.Add<M3>();
        });
    })
    .Build()
    .RunAsync();


public class M1 : IMiddleware
{
    private readonly ILogger<M1> _logger;

    public M1(ILogger<M1> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpRequest request, HttpResponse response, RequestDelegate next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("In M1");
        await next(request, response);
    }
}

public class M2 : IMiddleware
{
    private readonly ILogger<M2> _logger;

    public M2(ILogger<M2> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpRequest request, HttpResponse response, RequestDelegate next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("In M2");
        await next(request, response);
    }
}

public class M3 : IMiddleware
{
    private readonly ILogger<M3> _logger;

    public M3(ILogger<M3> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpRequest request, HttpResponse response, RequestDelegate next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("In M3");
        await next(request, response);
    }
}