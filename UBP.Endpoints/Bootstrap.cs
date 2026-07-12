using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Reflection;

using UBP.Endpoints.Interfaces;

namespace UBP.Endpoints;

public static class Bootstrap
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.TryAddEnumerable(GetImplementationDescriptors<IEndpoint>());
        services.TryAddEnumerable(GetImplementationDescriptors<IEndpointGroup>());
        services.TryAddEnumerable(GetGroupHandlerDescriptors());

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var endpoints = builder.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        var groups = builder.ServiceProvider.GetRequiredService<IEnumerable<IEndpointGroup>>();

        foreach (var group in groups)
        {
            var routeGroup = builder.MapGroup(group.Prefix);

            var handlerInterfaceType = typeof(IGroupEndpoint<>).MakeGenericType(group.GetType());
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerInterfaceType);
            var handlers = (IEnumerable<IEndpoint>)builder.ServiceProvider.GetRequiredService(handlersType);

            foreach (var handler in handlers)
            {
                handler.MapEndpoint(routeGroup);
            }
        }

        return builder;
    }

    private static IEnumerable<ServiceDescriptor> GetImplementationDescriptors<TService>()
    {
        var assembly = Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("Unable to resolve the entry assembly.");

        return assembly.GetTypes()
            .Where(static type => type is { IsAbstract: false, IsInterface: false } && typeof(TService).IsAssignableFrom(type))
            .Select(static type => ServiceDescriptor.Transient(typeof(TService), type));
    }

    private static IEnumerable<ServiceDescriptor> GetGroupHandlerDescriptors()
    {
        var assembly = Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("Unable to resolve the entry assembly.");

        return assembly.GetTypes()
            .Where(static type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(static type => type.GetInterfaces()
                .Where(static @interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IGroupEndpoint<>))
                .Select(@interface => ServiceDescriptor.Transient(@interface, type)));
    }
}
