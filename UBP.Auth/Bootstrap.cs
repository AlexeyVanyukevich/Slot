using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenIddict.Validation;

namespace UBP.Auth;

public static class Bootstrap
{
    public static IServiceCollection AddIamAuthentication(this IServiceCollection services, Action<IamAuthenticationOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);

        services.AddOptions<OpenIddictValidationOptions>()
            .Configure<IOptions<IamAuthenticationOptions>>((validationOptions, iamOptions) =>
            {
                validationOptions.Issuer = new Uri(iamOptions.Value.Authority);
                validationOptions.Audiences.UnionWith([iamOptions.Value.Audience]);
            });

        services.AddOpenIddict()
            .AddValidation(validationOptions =>
            {
                validationOptions.UseSystemNetHttp();
                validationOptions.UseAspNetCore();
            });

        return services;
    }
}
