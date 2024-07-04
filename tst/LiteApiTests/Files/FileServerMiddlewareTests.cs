using LiteApi.Files;
using Microsoft.Extensions.Options;
using Mockable.Core;
using System.Text;

namespace LiteApiTests.Files;

internal class FileServiceMiddlewareMockConfigurators
{
    public Mock<IOptions<LiteApiConfiguration>> ConfigOptions { get; set; } = null!;
    public Mock<IFileHelper> FileHelper { get; set; } = null!;
}

public class FileServerMiddlewareTests
{
    [Fact]
    public async Task FileTest()
    {
        // Arrange
        var factory = new ServiceFactory();
        var middleware = factory.Create<FileServerMiddleware, FileServiceMiddlewareMockConfigurators>(
            out var configurators,
            new NamedParameter { Name = "root", Value = "UnitTestRoot" }
        );

        var path = Path.Combine("UnitTestRoot", "index.html");

        var config = new LiteApiConfiguration
        {
            MimeTypes =
            [
                new MimeType { Extension = ".html", Type = "text/html" },
                new MimeType { Extension = ".css", Type = "text/css" }
            ]
        };

        var fileContent = "Hello world";
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        var responseHeaders = new HttpHeaderCollection();

        var responseMock = new Mock<IHttpResponse>();

        configurators.ConfigOptions.Setup(m => m.Value).Returns(config);
        configurators.FileHelper.Setup(m => m.FileExists(path)).Returns(true);
        configurators.FileHelper.Setup(m => m.ReadAllBytesAsync(path, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileBytes);
        responseMock.Setup(m => m.Headers).Returns(responseHeaders);

        var request = new HttpRequest("GET / HTTP/1.1");

        // Act
        await middleware.InvokeAsync(request, responseMock.Object, (request, response) => Task.CompletedTask, CancellationToken.None);

        // Assert
        responseMock.Verify(m => m.SetContent(fileBytes));
        Assert.Single(responseHeaders);
        Assert.Equal("Content-Type", responseHeaders.Single().Name);
        Assert.Equal("text/html", responseHeaders.Single().Value);
    }

    [Fact]
    public async Task NotFoundTest()
    {
        // Arrange
        var factory = new ServiceFactory();
        var middleware = factory.Create<FileServerMiddleware, FileServiceMiddlewareMockConfigurators>(
            out var configurators,
            new NamedParameter { Name = "root", Value = "UnitTestRoot" }
        );

        var path = Path.Combine("UnitTestRoot", "index.html");


        var responseMock = new Mock<IHttpResponse>();
        configurators.FileHelper.Setup(m => m.FileExists(path)).Returns(false);
        var request = new HttpRequest("GET / HTTP/1.1");

        // Act
        await middleware.InvokeAsync(request, responseMock.Object, (request, response) => Task.CompletedTask, CancellationToken.None);

        // Assert
        responseMock.Verify(m => m.NotFound());
    }
}
