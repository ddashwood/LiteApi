using System.Text;

namespace LiteApiTests;

public class HttpRequestFactoryTests
{
    [Fact]
    public void CreateRequestTest()
    {
        // Arrange
        var serviceFactory = new ServiceFactory();
        var factory = serviceFactory.Create<HttpRequestFactory>();
        using var stream = new MemoryStream();

        var requestText = "GET / HTTP/1.1\r\n\r\n";
        var bytes = Encoding.UTF8.GetBytes(requestText);
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        // Act
        var result = factory.CreateHttpRequest(stream);

        // Assert
        Assert.Equal("GET", result.Method);
        Assert.Equal("/", result.Target);
        Assert.Equal("HTTP/1.1", result.Version);
    }
}
