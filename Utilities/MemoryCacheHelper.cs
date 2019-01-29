using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Teknik.Utilities
{
    public static class MemoryCacheHelper
    {
        public static bool GetCacheValue<T>(this IMemoryCache cache, string key, out T value)
        {
            return cache.TryGetValue(key, out value);
        }

        public static bool GetCacheValue(this IMemoryCache cache, string key, out object value)
        {
            return cache.TryGetValue(key, out value);
        }

        public static void AddToCache(this IMemoryCache cache, string key, object value, TimeSpan expiration)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);

            cache.AddToCache(key, value, cacheOptions);
        }

        public static void AddToCache(this IMemoryCache cache, string key, object value, MemoryCacheEntryOptions options)
        {
            cache.Set(key, value, options);
        }
    }
}
