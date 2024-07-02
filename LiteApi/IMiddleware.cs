namespace LiteApi;

public delegate Task RequestDelegate(HttpRequest request, HttpResponse response);

public interface IMiddleware
{
    Task InvokeAsync(HttpRequest request, HttpResponse response, RequestDelegate next, CancellationToken cancellationToken);
}
