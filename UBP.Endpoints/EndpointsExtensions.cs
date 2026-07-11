using Microsoft.AspNetCore.Routing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UBP.Endpoints;

public static class EndpointsExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var endpointDescriptors = ServiceDescriptorExtensions.GetTransientDescriptors<IEndpoint>();

        services.TryAddEnumerable(endpointDescriptors);

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var endpoints = builder.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return builder;
    }
}
