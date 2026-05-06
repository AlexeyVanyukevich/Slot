namespace Slot.Cache.Interfaces;

public interface ICache
{
    Task<TItem?> GetOrDefaultAsync<TItem>(object key);
    Task<TItem> PutAsync<TItem>(object key, TItem value);
}
