using System.Net;

namespace LiteApi;

public interface IHttpResponse
{
    byte[]? Content { get; }
    HttpHeaderCollection Headers { get; }
    bool ResponseSet { get; }
    HttpStatusCode StatusCode { get; }

    void NotFound();
    void SetContent(byte[] content);
}