using Microsoft.Extensions.Caching.Memory;

using UBP.Cache.Abstractions;

namespace UBP.Cache.InMemory;

internal sealed class InMemoryCache(IMemoryCache memoryCache, CacheOptions cacheOptions) : ICache
{
    public Task<TItem> PutAsync<TItem>(object key, TItem value)
    {
        return Task.FromResult(memoryCache.Set(key, value, TimeSpan.FromSeconds(cacheOptions.TtlSeconds)));
    }

    public Task<TItem?> GetOrDefaultAsync<TItem>(object key)
    {
        if (memoryCache.TryGetValue(key, out TItem? value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult<TItem?>(default);
    }
}
