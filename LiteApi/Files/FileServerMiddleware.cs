
using Microsoft.Extensions.Logging;

namespace LiteApi.Files;

internal class FileServerMiddleware : IMiddleware
{
    private readonly ILogger<FileServerMiddleware> _logger;
    private readonly string _root;

    public FileServerMiddleware(ILogger<FileServerMiddleware> logger, string root)
    {
        _logger = logger;
        _root = root;
    }

    public async Task InvokeAsync(HttpRequest request, HttpResponse response, RequestDelegate next, CancellationToken cancellationToken)
    {
        var errorMessage = await ProcessFileRequestAsync(request, response, cancellationToken);

        if (errorMessage == null)
        {
            // All good
            return;
        }

        await next(request, response);
        if (!response.ResponseSet)
        {
            // Only if nothing else in the pipeline processed the response
            throw new InvalidOperationException(errorMessage);
        }
    }

    private async Task<string?> ProcessFileRequestAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        if (request.Resource == null)
        {
            return "No resource was specified";
        }

        var resource = request.Resource == "/" ? "index.html" : request.Resource;

        var pathParts = resource.Split('/');

        var path = _root;
        foreach (var pathPart in pathParts)
        {
            path = Path.Combine(path, pathPart);
        }

        if (!File.Exists(path))
        {
            return "Requested resource not found";
        }

        _logger.LogInformation("Serving file at path: {path}", path);

        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        content = RemoveBOM(content);
        response.SetContent(content);
        return null;
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
}
