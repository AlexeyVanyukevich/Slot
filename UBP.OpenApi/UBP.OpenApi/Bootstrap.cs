using Microsoft.Extensions.DependencyInjection;

namespace UBP.OpenApi;

public static class Bootstrap
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services,
        Action<OpenApiDocumentOptions> configure)
    {
        var options = new OpenApiDocumentOptions();
        configure(options);

        services.AddOpenApi(options.DocumentName, apiOptions =>
        {
            apiOptions.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title = options.Title;
                document.Info.Version = options.Version;
                document.Info.Description = options.Description;
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
