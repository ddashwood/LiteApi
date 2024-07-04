﻿
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiteApi.Files;

internal class FileServerMiddleware : IMiddleware
{
    private readonly ILogger<FileServerMiddleware> _logger;
    private readonly LiteApiConfiguration _config;
    private readonly string _root;

    public FileServerMiddleware(ILogger<FileServerMiddleware> logger, IOptions<LiteApiConfiguration> config, string root)
    {
        _logger = logger;
        _config = config.Value;
        _root = root;
    }

    public async Task InvokeAsync(HttpRequest request, HttpResponse response, RequestDelegate next, CancellationToken cancellationToken)
    {
        var success = await ProcessFileRequestAsync(request, response, cancellationToken);

        if (success)
        {
            return;
        }

        await next(request, response);
        if (!response.ResponseSet)
        {
            // Only if nothing else in the pipeline processed the response
            response.NotFound();
        }
    }

    private async Task<bool> ProcessFileRequestAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        if (request.Target == null)
        {
            throw new InvalidOperationException("No resource was specified");
        }

        var resource = request.Target == "/" ? "index.html" : request.Target;

        var pathParts = resource.Split('/');

        var path = _root;
        foreach (var pathPart in pathParts)
        {
            path = Path.Combine(path, pathPart);
        }

        if (!File.Exists(path))
        {
            return false;
        }

        _logger.LogInformation("Serving file at path: {path}", path);

        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        content = RemoveBOM(content);
        response.SetContent(content);

        var dotIndex = resource.LastIndexOf(".");
        if (dotIndex != -1)
        {
            var extension = resource.Substring(dotIndex);
            if (_config.MimeTypesByExtension.Value.TryGetValue(extension, out var mimeType))
            {
                response.Headers.Add("Content-Type", mimeType.Type);
            }
        }

        return true;
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
