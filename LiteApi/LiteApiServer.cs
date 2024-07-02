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

                    string requestLine = ReadLine(stream);
                    bool firstLine = true;
                    while (!string.IsNullOrWhiteSpace(requestLine))
                    {
                        if (firstLine)
                        {
                            _logger.LogInformation("Received request: " + requestLine);
                        }
                        else
                        {
                            _logger.LogDebug("Header: " + requestLine);
                        }
                        firstLine = false;
                        requestLine = ReadLine(stream);
                    }

                    var response = "<html><head><title>Test</title><body><h1>Test</h1>Test page</body></html>";
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, cancellationToken);
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
}
