namespace LiteApi;

public class HttpResponse
{
    internal byte[]? Content { get; private set; }
    internal bool ResponseSet { get; private set; }

    internal HttpResponse()
    { }

    public void SetContent(byte[] content)
    {
        Content = content;
        ResponseSet = true;
    }
}
