using System.Net;

namespace LiteApi;

public class HttpResponse
{
    internal byte[]? Content { get; private set; }
    internal bool ResponseSet { get; private set; }
    internal HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;

    internal HttpResponse()
    { }

    public void SetContent(byte[] content)
    {
        Content = content;
        ResponseSet = true;
    }

    public void NotFound()
    {
        StatusCode = HttpStatusCode.NotFound;
        ResponseSet = true;
    }
}
