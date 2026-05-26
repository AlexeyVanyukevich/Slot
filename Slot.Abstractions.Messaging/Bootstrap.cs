using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Slot.Abstractions.Messaging;

public static class Bootstrap
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        services.AddScoped<ISender, Sender>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = GetTypes(assembly);
            RegisterHandlers(services, handlerTypes, typeof(IRequestHandler<>));
            RegisterHandlers(services, handlerTypes, typeof(IRequestHandler<,>));
        }

        return services;
    }

    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(static t => t is { IsAbstract: false, IsInterface: false });
    }

    private static void RegisterHandlers(IServiceCollection services, IEnumerable<Type> handlerTypes, Type openGenericInterface)
    {
        foreach (var handlerType in handlerTypes)
        {
            var matchedInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);

            foreach (var iface in matchedInterfaces)
            {
                services.AddScoped(iface, handlerType);
            }
        }
    }
}
