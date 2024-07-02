namespace LiteApi;

public class HttpRequest
{
    public string? Method { get; }
    public string? Target { get; }
    public string? Version { get; }

    internal HttpRequest(string requestLine)
    {
        var parts = requestLine.Split(' ');

        Method = parts.Length >= 1 ? parts[0] : null;
        Target = parts.Length >= 2 ? parts[1] : null;
        Version = parts.Length >= 3 ? parts[2] : null;
    }
}
