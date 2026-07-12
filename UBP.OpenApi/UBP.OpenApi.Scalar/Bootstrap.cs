using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Scalar.AspNetCore;

namespace UBP.OpenApi.Scalar;

public static class Bootstrap
{
    public static IEndpointRouteBuilder MapOpenApiDocumentation(
        this IEndpointRouteBuilder app)
    {
        var env = app.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (!env.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();
        app.MapScalarApiReference();
        return app;
    }
}
