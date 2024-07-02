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

    public LiteApiServer(ILogger<LiteApiServer> logger,
                         int port,
                         Action<MiddlewarePipelineConfiguration> configurePipeline,
                         IServiceProvider services)
    {
        _logger = logger;
        _port = port;
        _services = services;

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
                    using var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    await using var stream = client.GetStream();

                    HttpRequest request = null!;
                    string requestLine = ReadLine(stream);
                    bool firstLine = true;
                    while (!string.IsNullOrWhiteSpace(requestLine))
                    {
                        if (firstLine)
                        {
                            _logger.LogInformation("Received request: " + requestLine);
                            request = new HttpRequest(requestLine);
                        }
                        else
                        {
                            _logger.LogDebug("Header: " + requestLine);
                        }
                        firstLine = false;
                        requestLine = ReadLine(stream);
                    }

                    var response = new HttpResponse();
                    await ProcessRequestAsync(request, response, cancellationToken);
                    await ProcessResponseAsync(response, stream, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving request");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private string ReadLine(Stream stream)
    {
        StringBuilder sb = new StringBuilder();

        int ch;
        do
        {
            ch = stream.ReadByte();
            if (ch != 10 && ch != 13)
            {
                sb.Append((char)ch);
            }
        } while (ch != 10);

        return sb.ToString();
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

    private async Task ProcessResponseAsync(HttpResponse response, NetworkStream stream, CancellationToken cancellationToken)
    {
        await stream.WriteAsync(response.Content, cancellationToken);
    }
}
