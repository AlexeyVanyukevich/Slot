using Microsoft.Extensions.DependencyInjection;

using UBP.Booking.API.Setup;

namespace UBP.Booking.API.Extensions;

internal static class OptionsExtensions
{
    internal static IServiceCollection ConfigureAppOptions(this IServiceCollection services)
    {
        services.ConfigureOptions<DbOptionsSetup>();

        return services;
    }
}
