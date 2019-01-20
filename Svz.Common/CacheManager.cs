using System;
using StackExchange.Redis;
using ZeroFormatter;

namespace Svz.Common
{
    public class CacheManager<TKey, TValue> : ICacheManager<TKey, TValue>
    {
        private readonly ICacheProvider<TKey, TValue> _cacheProvider;
        private readonly ICacheValueSource<TKey, TValue> _cacheValueSource;

        public CacheManager(ICacheValueSource<TKey, TValue> cacheValueSource, ICacheProvider<TKey, TValue>
            cacheProvider)
        {
            _cacheValueSource = cacheValueSource;
            _cacheProvider = cacheProvider;
        }

        public TValue Get(TKey key)
        {
            var resp = _cacheProvider.Get(key);
            if (resp != null)
                return resp;

            resp = _cacheValueSource.Get(key);
            _cacheProvider.Set(key, resp);

            return resp;
        }
    }

    public interface ICacheManager<TKey, TValue>
    {
        TValue Get(TKey key);
    }

    public interface ICacheProvider<TKey, TValue> : IDisposable
    {
        TValue Get(TKey key);
        void Set(TKey key, TValue value);
    }

    public class RedisCacheProvider<TKey, TValue> : ICacheProvider<TKey, TValue>
    {
        private readonly string _cacheName;
        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _database;
        private readonly int _ttlSeconds;

        public RedisCacheProvider(SvzConfig config, string cacheName, int ttlSeconds)
        {
            _connection = ConnectionMultiplexer.Connect(config.Redis);
            _database = _connection.GetDatabase();
            _cacheName = cacheName;
            _ttlSeconds = ttlSeconds;
        }

        public TValue Get(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var raw = _database.StringGet(GetRedisKey(key));

            if (raw.IsNullOrEmpty)
                return default;

            return ZeroFormatterSerializer.Deserialize<TValue>(raw);
        }

        public void Set(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var data = ZeroFormatterSerializer.Serialize(value);
            var ts = new TimeSpan(0, 0, _ttlSeconds);
            _database.StringSet(GetRedisKey(key), data, ts);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        private string GetRedisKey(object rawKey)
        {
            return $"{_cacheName}:{rawKey}";
        }
    }

    public interface ICacheValueSource<TKey, TValue>
    {
        TValue Get(TKey key);
    }
}