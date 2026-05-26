using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Slot.Abstractions.Messaging;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new();

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        return (Task<TResponse>)InvokeHandler(handlerType, request, cancellationToken);
    }

    public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        return (Task)InvokeHandler(handlerType, request, cancellationToken);
    }

    private object InvokeHandler(Type handlerType, object message, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService(handlerType);
        var method = _methodCache.GetOrAdd(handlerType, static t => t.GetMethod(nameof(IRequestHandler<>.HandleAsync))!);
        return method.Invoke(handler, [message, cancellationToken])!;
    }
}
