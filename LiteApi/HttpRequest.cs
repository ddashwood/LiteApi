namespace LiteApi;

public class HttpRequest
{
    public string? Verb { get; }
    public string? Resource { get; }
    public string? Version { get; }

    internal HttpRequest(string requestLine)
    {
        var parts = requestLine.Split(' ');

        Verb = parts.Length >= 1 ? parts[0] : null;
        Resource = parts.Length >= 2 ? parts[1] : null;
        Version = parts.Length >= 3 ? parts[2] : null;
    }
}
