using Microsoft.Extensions.DependencyInjection;

using UBP.Storage.Abstractions;

namespace UBP.Storage.Local;

public static class Bootstrap
{
    public static IServiceCollection AddLocalAssetStorage(
        this IServiceCollection services,
        Func<IServiceProvider, AssetStorageOptions> assetStorageOptionsFactory,
        Func<IServiceProvider, LocalAssetStorageOptions> localOptionsFactory)
    {
        services.AddSingleton(assetStorageOptionsFactory);
        services.AddSingleton(localOptionsFactory);
        services.AddSingleton<IAssetStorage, LocalAssetStorage>();

        return services;
    }
}
