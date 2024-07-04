namespace LiteApi;

public delegate Task RequestDelegate(IHttpRequest request, IHttpResponse response);

public interface IMiddleware
{
    Task InvokeAsync(IHttpRequest request, IHttpResponse response, RequestDelegate next, CancellationToken cancellationToken);
}
