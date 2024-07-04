namespace LiteApiTests;

public class HttpRequestTests
{
    [Fact]
    public void HttpRequestCtorTest()
    {
        // Arrange

        // Act
        var request = new HttpRequest("GET /testresource HTTP/1.1");

        // Assert
        Assert.Equal("GET", request.Method);
        Assert.Equal("/testresource", request.Target);
        Assert.Equal("HTTP/1.1", request.Version);
    }
}
