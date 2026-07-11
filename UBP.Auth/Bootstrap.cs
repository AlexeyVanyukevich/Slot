using Microsoft.Extensions.DependencyInjection;

namespace UBP.Auth;

public static class Bootstrap
{
    public static IServiceCollection AddIamAuthentication(this IServiceCollection services, Action<IamAuthenticationOptions> configure)
    {
        var options = new IamAuthenticationOptions();
        configure(options);

        services.AddOpenIddict()
            .AddValidation(validationOptions =>
            {
                validationOptions.SetIssuer(options.Authority);
                validationOptions.AddAudiences(options.Audience);

                validationOptions.UseSystemNetHttp();
                validationOptions.UseAspNetCore();
            });

        return services;
    }
}
