namespace LiteApi;

public class HttpResponse
{
    internal byte[] Content { get; }

    public HttpResponse(byte[] content)
    {
        Content = content;
    }
}
