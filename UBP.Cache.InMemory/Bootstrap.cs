using Microsoft.Extensions.DependencyInjection;

using UBP.Cache.Abstractions;

namespace UBP.Cache.InMemory;

public static class Bootstrap
{
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, Func<IServiceProvider, CacheOptions> cacheOptionsFactory)
    {
        services.AddSingleton(cacheOptionsFactory);

        services.AddMemoryCache();
        services.AddSingleton<ICache, InMemoryCache>();

        return services;
    }
}
