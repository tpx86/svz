namespace Svz.Common
{
    public class CacheResponse<TValue>
    {
        public TValue Value { get; }
        public bool FromCache { get; }

        private CacheResponse(TValue value, bool fromCache)
        {
            Value = value;
            FromCache = fromCache;
        }

        public static CacheResponse<TValue> Cached(TValue value)
        {
            return new CacheResponse<TValue>(value, true);
        }

        public static CacheResponse<TValue> NonCached(TValue value)
        {
            return new CacheResponse<TValue>(value, false);
        }
    }
}