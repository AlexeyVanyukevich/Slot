using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UBP.OpenApi;

public static class Bootstrap
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services,
        string documentName,
        Action<OpenApiDocumentOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);

        services.AddOpenApi(documentName, apiOptions =>
        {
            apiOptions.AddDocumentTransformer((document, context, ct) =>
            {
                OpenApiDocumentOptions options = context.ApplicationServices.GetRequiredService<IOptions<OpenApiDocumentOptions>>().Value;

                document.Info.Title = options.Title;
                document.Info.Version = options.Version;
                document.Info.Description = options.Description;
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
