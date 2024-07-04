namespace LiteApi;

public class HttpHeader
{
    public string Name { get; }
    public string Value { get; set; } = "";

    public HttpHeader(string name)
    {
        Name = name;
    }

    public HttpHeader(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
