using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LiteApi;

internal class LiteApiServer : IHostedService
{
    private TcpListener _listener = null!;

    private ILogger<LiteApiServer> _logger;
    private int _port;

    public LiteApiServer(ILogger<LiteApiServer> logger, int port)
    {
        _logger = logger;
        _port = port;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        removeListener();
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();

        _ = Run(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        removeListener();
        return Task.CompletedTask;
    }

    private void removeListener()
    {
        _listener?.Stop();
        _listener?.Dispose();
    }

    public async Task Run(CancellationToken cancellationToken)
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

                    var response = await ProcessRequestAsync(request, cancellationToken);
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

    private async Task<HttpResponse> ProcessRequestAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.Resource == null)
        {
            throw new InvalidOperationException("No resource was specified");
        }

        var resource = request.Resource == "/" ? "index.html" : request.Resource;

        var pathParts = resource.Split('/');

        var path = "wwwroot";
        foreach (var pathPart in pathParts)
        {
            path = Path.Combine(path, pathPart);
        }

        if (!File.Exists(path))
        {
            throw new InvalidOperationException("Requested resource not found");
        }

        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        content = RemoveBOM(content);
        return new HttpResponse(content);
    }

    private byte[] RemoveBOM(byte[] bytes)
    {
        if (bytes.Length < 3)
        {
            return bytes;
        }

        if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return bytes.Skip(3).ToArray();
        }

        return bytes;
    }

    private async Task ProcessResponseAsync(HttpResponse response, NetworkStream stream, CancellationToken cancellationToken)
    {
        await stream.WriteAsync(response.Content, cancellationToken);
    }
}
