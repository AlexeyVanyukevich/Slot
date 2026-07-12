using UBP.Storage.API.Setup;

namespace UBP.Storage.API.Extensions;

internal static class OptionsExtensions
{
    internal static IServiceCollection ConfigureAppOptions(this IServiceCollection services)
    {
        services.ConfigureOptions<DbOptionsSetup>();
        services.ConfigureOptions<AssetStorageOptionsSetup>();
        services.ConfigureOptions<LocalAssetStorageOptionsSetup>();

        return services;
    }
}
