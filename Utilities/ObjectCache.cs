using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public class ObjectCache
    {
        private readonly int _defaultCacheTime;
        private static object _cacheLock = new object();

        private readonly static ConcurrentDictionary<string, CacheEntry> _objectCache = new ConcurrentDictionary<string, CacheEntry>();

        public ObjectCache(int defaultCacheTime)
        {
            _defaultCacheTime = defaultCacheTime;
        }

        public T AddOrGetObject<T>(string key, Func<string, T> getObjectFunc)
        {
            return AddOrGetObject(key, new TimeSpan(0, 0, _defaultCacheTime), getObjectFunc);
        }

        public T AddOrGetObject<T>(string key, TimeSpan cacheTime, Func<string, T> getObjectFunc)
        {
            T foundObject;
            lock (_cacheLock)
            {
                if (_objectCache.TryGetValue(GenerateKey<T>(key), out var result) &&
                    CacheValid(result))
                {
                    if (result.RollingExpiration)
                    {
                        result.LastUpdate = DateTime.UtcNow;
                    }
                    return (T)result.Data;
                }
                else
                {
                    foundObject = getObjectFunc(key);
                    // Update the cache for this key
                    if (foundObject != null)
                        UpdateObject(key, foundObject, cacheTime);
                }
            }

            return foundObject;
        }

        public void UpdateObject<T>(string key, T data)
        {
            UpdateObject(key, data, null, DateTime.UtcNow);
        }

        public void UpdateObject<T>(string key, T data, TimeSpan cacheTime)
        {
            UpdateObject(key, data, cacheTime, DateTime.UtcNow);
        }

        public void UpdateObject<T>(string key, T data, TimeSpan? cacheTime, DateTime lastUpdate)
        {
            if (_objectCache.TryGetValue(GenerateKey<T>(key), out var result))
            {
                result.Data = data;
                if (cacheTime.HasValue)
                    result.CacheTime = cacheTime.Value;
                result.LastUpdate = lastUpdate;
            }
            else
            {
                if (!cacheTime.HasValue)
                    cacheTime = new TimeSpan(0, 0, _defaultCacheTime);
                _objectCache[GenerateKey<T>(key)] = CreateCacheEntry(data, cacheTime.Value, lastUpdate);
            }
        }

        public void DeleteObject<T>(string key)
        {
            _objectCache.TryRemove(GenerateKey<T>(key), out _);
        }

        public bool CacheValid(CacheEntry entry)
        {
            return entry.LastUpdate > DateTime.UtcNow.Subtract(entry.CacheTime);
        }

        public void CleanCache()
        {
            lock (_cacheLock)
            {
                foreach (var obj in _objectCache)
                {
                    if (!CacheValid(obj.Value))
                        _objectCache.TryRemove(obj.Key, out _);
                }
            }
        }

        public string GenerateKey<T>(string key)
        {
            var typeKey = typeof(T).ToString();
            return $"{typeKey}.{key}";
        }

        public CacheEntry CreateCacheEntry<T>(T data, TimeSpan cacheTime, DateTime lastUpdate)
        {
            return new CacheEntry(false, cacheTime, lastUpdate, data);
        }
    }
}
