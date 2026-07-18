using Infrastructure.Cache;
using Infrastructure.Service;

namespace ERP_API.Infrastructure.Cache
{
    public interface IProductCache
    {
        /// <summary>Bumps cache generation so all product keys stop matching.</summary>
        void Invalidate();

        bool TryGet<T>(string relativeKey, out T? value);
        void Set<T>(string relativeKey, T value, int? expireInSeconds = null);
    }

    /// <summary>
    /// Versioned product cache on top of <see cref="IAppMemoryCache"/>.
    /// Invalidate() bumps a generation counter so old entries are ignored
    /// (they expire via TTL as a safety net).
    /// </summary>
    public class ProductCache : IProductCache, IScopped
    {
        public const int DefaultTtlSeconds = 300; // 5 minutes
        private const string VersionKey = "products:cache-version";

        private readonly IAppMemoryCache _cache;

        public ProductCache(IAppMemoryCache cache)
        {
            _cache = cache;
        }

        private int CurrentVersion
        {
            get
            {
                if (!_cache.IsExist(VersionKey))
                    return 0;
                try
                {
                    return _cache.Get<int>(VersionKey);
                }
                catch
                {
                    return 0;
                }
            }
        }

        private string FullKey(string relativeKey) => $"products:v{CurrentVersion}:{relativeKey}";

        public void Invalidate()
        {
            var next = CurrentVersion + 1;
            _cache.Set(VersionKey, next);
        }

        public bool TryGet<T>(string relativeKey, out T? value)
        {
            value = default;
            var key = FullKey(relativeKey);
            if (!_cache.IsExist(key))
                return false;

            try
            {
                value = _cache.Get<T>(key);
                return value is not null || typeof(T).IsValueType;
            }
            catch
            {
                return false;
            }
        }

        public void Set<T>(string relativeKey, T value, int? expireInSeconds = null)
        {
            if (value is null) return;
            var ttl = expireInSeconds ?? DefaultTtlSeconds;
            _cache.Set(FullKey(relativeKey), value, ttl);
        }
    }
}
