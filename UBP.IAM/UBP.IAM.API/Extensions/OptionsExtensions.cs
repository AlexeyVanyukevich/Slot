using UBP.IAM.API.Setup;

namespace UBP.IAM.API.Extensions;

internal static class OptionsExtensions
{
    internal static IServiceCollection ConfigureAppOptions(this IServiceCollection services)
    {
        services.ConfigureOptions<DbOptionsSetup>();

        return services;
    }
}
