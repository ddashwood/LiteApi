using Microsoft.Extensions.DependencyInjection;

namespace LiteApi;

public class MiddlewarePipelineConfiguration
{
    private readonly Stack<Func<IServiceProvider, IMiddleware>> _middlewaresCreators = new Stack<Func<IServiceProvider, IMiddleware>>();
    internal IEnumerable<Func<IServiceProvider, IMiddleware>> MiddlewareCreators => _middlewaresCreators;



    public void Add(Type middlewareType)
    {
        if (!typeof(IMiddleware).IsAssignableFrom(middlewareType))
        {
            throw new InvalidOperationException("Attempt to add middleware which does not implement IMiddleware");
        }

        _middlewaresCreators.Push(services => (IMiddleware)services.GetRequiredService(middlewareType));
    }

    public void Add<TMiddleware>() where TMiddleware : IMiddleware
    {
        _middlewaresCreators.Push(services => services.GetRequiredService<TMiddleware>());
    }

    public void Add(Func<IServiceProvider, IMiddleware> middlewareCreator)
    {
        _middlewaresCreators.Push(middlewareCreator);
    }
}
