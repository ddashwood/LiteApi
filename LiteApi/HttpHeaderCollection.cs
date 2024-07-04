using System.Collections;

namespace LiteApi;

public class HttpHeaderCollection : IEnumerable<HttpHeader>
{
    private readonly Dictionary<string, HttpHeader> _headers = new ();

    public HttpHeader this[string name]
    {
        get => _headers [name];
    }

    public void Add(string name, string value)
    {
        if (_headers.ContainsKey (name))
        {
            throw new ArgumentException("A header with the specified name already exists", nameof(name));
        }

        _headers.Add(name, new HttpHeader(name, value));
    }

    public IEnumerator<HttpHeader> GetEnumerator() => _headers.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _headers.Values.GetEnumerator();
}
