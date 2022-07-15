using dsi.common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace dsi.services
{
    public class ChacheServise : IChacheServise
    {
        readonly TimeSpan _lifetime = TimeSpan.FromHours(1);
        readonly IMemoryCache _memoryCache;

        public ChacheServise(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetFromCacheAsync<T>(string key)
        {
            return await Task.Run(() => _memoryCache.Get<T>(key)).ConfigureAwait(false);
        }

        public T GetFromCache<T>(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        public async Task PutToCacheAsync<T>(string key, T value, TimeSpan? lifeTime)
        {
            _ = await Task.Run(() => _ = _memoryCache.Set(key, value, lifeTime ?? _lifetime)).ConfigureAwait(false);
        }
    }
}
