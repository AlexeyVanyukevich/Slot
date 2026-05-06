using Microsoft.Extensions.DependencyInjection;

using Slot.Cache.Interfaces;
using Slot.Cache.Options;

namespace Slot.Cache.Memory;

public static class Bootstrap
{
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, Func<IServiceProvider, CacheOptions> cacheOptionsFactory)
    {
        services.AddSingleton(cacheOptionsFactory);

        services.AddMemoryCache();
        services.AddSingleton<ICache, MemoryCache>();

        return services;
    }
}
