using Microsoft.Extensions.Logging;
using System.Text;

namespace LiteApi;

internal class HttpRequestFactory
{
    private readonly ILogger<HttpRequestFactory> _logger;

    public HttpRequestFactory(ILogger<HttpRequestFactory> logger)
    {
        _logger = logger;
    }

    public HttpRequest CreateHttpRequest(Stream stream)
    {
        HttpRequest? request = null;
        bool firstLine = true;
        string line = ReadLine(stream);
        while (!string.IsNullOrWhiteSpace(line))
        {
            if (firstLine)
            {
                _logger.LogInformation("Received request: " + line);
                request = new HttpRequest(line);
            }
            else
            {
                _logger.LogDebug("Header: " + line);
            }
            firstLine = false;
            line = ReadLine(stream);
        }

        if (request == null)
        {
            throw new InvalidOperationException("No request found");
        }

        return request;
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
