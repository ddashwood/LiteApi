using System.Text;
using System.Threading.Tasks.Sources;
using static System.Net.Mime.MediaTypeNames;

namespace LiteApiTests;

public class HttpResponseProcessorTests
{
    [Fact]
    public async Task ContentTest()
    {
        // Arrange
        var factory = new ServiceFactory();
        var processor = factory.Create<HttpResponseProcessor>();
        var stream = new MemoryStream();
        var response = new HttpResponse();

        var text = "Hello world";
        var bytes = Encoding.UTF8.GetBytes(text);
        response.SetContent(bytes);

        // Act
        await processor.ProcessResponseAsync(response, stream, CancellationToken.None);
        var responseBytes = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(responseBytes, 0, responseBytes.Length);
        var responseText = Encoding.UTF8.GetString(responseBytes);

        // Assert
        var expected = $"HTTP/1.1 200 OK\r\nContent-Length: {text.Length}\r\n\r\n{text}";
        Assert.Equal(expected, responseText);
    }

    [Fact]
    public async Task NotFoundTest()
    {
        // Arrange
        var factory = new ServiceFactory();
        var processor = factory.Create<HttpResponseProcessor>();
        var stream = new MemoryStream();
        var response = new HttpResponse();

        response.NotFound();

        // Act
        await processor.ProcessResponseAsync(response, stream, CancellationToken.None);
        var responseBytes = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(responseBytes, 0, responseBytes.Length);
        var responseText = Encoding.UTF8.GetString(responseBytes);

        // Assert
        var expected = $"HTTP/1.1 404 NotFound\r\n\r\n";
        Assert.Equal(expected, responseText);
    }
}
