namespace dsi.common.Interfaces
{
    public interface IChacheServise
    {
        public Task PutToCacheAsync<T>(string key, T value, TimeSpan? lifeTime);

        public Task<T> GetFromCacheAsync<T>(string key);

        public T GetFromCache<T>(string key);
    }
}
