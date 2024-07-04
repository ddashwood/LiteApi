namespace LiteApi;

public interface IHttpRequest
{
    string? Method { get; }
    string? Target { get; }
    string? Version { get; }
}