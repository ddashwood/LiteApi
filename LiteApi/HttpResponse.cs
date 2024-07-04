using System.Net;

namespace LiteApi;

public class HttpResponse
{
    private byte[]? _content;
    public byte[]? Content => _content?.ToArray();
    public bool ResponseSet { get; private set; }
    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;
    public HttpHeaderCollection Headers { get; } = new();

    internal HttpResponse()
    { }

    public void SetContent(byte[] content)
    {
        _content = content;
        ResponseSet = true;
    }

    public void NotFound()
    {
        StatusCode = HttpStatusCode.NotFound;
        ResponseSet = true;
    }
}
