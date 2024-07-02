using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LiteApi;

internal class LiteApiServer : IHostedService
{
    private TcpListener _listener = null!;

    private readonly ILogger<LiteApiServer> _logger;
    private readonly int _port;
    private readonly MiddlewarePipelineConfiguration _config;
    private readonly IServiceProvider _services;
    private readonly HttpRequestFactory _httpRequestFactory;
    private readonly HttpResponseProcessor _httpResponseProcessor;

    public LiteApiServer(ILogger<LiteApiServer> logger,
                         int port,
                         Action<MiddlewarePipelineConfiguration> configurePipeline,
                         IServiceProvider services,
                         HttpRequestFactory httpRequestFactory,
                         HttpResponseProcessor httpResponseProcessor)
    {
        _logger = logger;
        _port = port;
        _services = services;
        _httpRequestFactory = httpRequestFactory;
        _httpResponseProcessor = httpResponseProcessor;

        _config = new MiddlewarePipelineConfiguration();
        configurePipeline(_config);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RemoveListener();
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();

        _ = Run(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        RemoveListener();
        return Task.CompletedTask;
    }

    private void RemoveListener()
    {
        _listener?.Stop();
        _listener?.Dispose();
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    var request = (client, cancellationToken);
                    ThreadPool.QueueUserWorkItem(ProcessClientAsync, request, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving request");
                }
            }
        }
        catch (OperationCanceledException)
        { }
    }

    private async void ProcessClientAsync((TcpClient client, CancellationToken cancellationToken) clientAndToken)
    {
        try
        {
            var stream = clientAndToken.client.GetStream();
            var request = _httpRequestFactory.CreateHttpRequest(stream);

            var response = new HttpResponse();
            await ProcessRequestAsync(request, response, clientAndToken.cancellationToken);
            await _httpResponseProcessor.ProcessResponseAsync(response, stream, clientAndToken.cancellationToken);
        }
        catch (OperationCanceledException)
        { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client request");
        }
        finally
        {
            clientAndToken.client?.Dispose();
        }
    }

    private async Task ProcessRequestAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();

        RequestDelegate del = (request, response) => Task.CompletedTask;
        foreach (var middlewareCreator in _config.MiddlewareCreators)
        {
            var previous = del;
            del = (request, response) =>
            {
                var middleware = middlewareCreator(scope.ServiceProvider);
                return middleware.InvokeAsync(request, response, previous, cancellationToken);
            };
        }

        await del(request, response);
    }
}
