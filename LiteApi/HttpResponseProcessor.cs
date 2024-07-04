using System.Net.Sockets;
using System.Text;

namespace LiteApi;

internal class HttpResponseProcessor
{
    public async Task ProcessResponseAsync(HttpResponse response, NetworkStream stream, CancellationToken cancellationToken)
    {

        byte[] line;

        line = Encoding.UTF8.GetBytes($"HTTP/1.1 {(int)response.StatusCode} {response.StatusCode}\r\n");
        await stream.WriteAsync(line, cancellationToken);

        foreach (var header in response.Headers)
        {
            line = Encoding.UTF8.GetBytes($"{header.Name}: {header.Value}\r\n");
            await stream.WriteAsync(line, cancellationToken);
        }

        if (response.Content == null)
        {
            line = Encoding.UTF8.GetBytes("\r\n");
            await stream.WriteAsync(line, cancellationToken);
        }
        else
        {

            line = Encoding.UTF8.GetBytes($"Content-Length: {response.Content?.Length ?? 0}\r\n");
            await stream.WriteAsync(line, cancellationToken);

            line = Encoding.UTF8.GetBytes("\r\n");
            await stream.WriteAsync(line, cancellationToken);

            await stream.WriteAsync(response.Content, cancellationToken);
        }
    }
}
