using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public class ObjectCache
    {
        private readonly static Dictionary<string, Tuple<DateTime, object>> objectCache = new Dictionary<string, Tuple<DateTime, object>>();
        private readonly int _cacheSeconds;

        public ObjectCache(int cacheSeconds)
        {
            _cacheSeconds = cacheSeconds;
        }

        public T GetObject<T>(string key, Func<string, T> getObjectFunc)
        {
            T foundObject;
            var cacheDate = DateTime.UtcNow;
            if (objectCache.TryGetValue(key, out var result) &&
                result.Item1 > cacheDate.Subtract(new TimeSpan(0, 0, _cacheSeconds)))
            {
                return (T)result.Item2;
            }
            else
            {
                foundObject = getObjectFunc(key);
                // Update the cache for this key
                if (foundObject != null)
                    UpdateObject(key, foundObject, cacheDate);
            }

            return foundObject;
        }

        public void UpdateObject<T>(string key, T update)
        {
            UpdateObject(key, update, DateTime.UtcNow);
        }

        public void UpdateObject<T>(string key, T update, DateTime cacheTime)
        {
            objectCache[key] = new Tuple<DateTime, object>(cacheTime, update);
        }

        public void DeleteObject(string key)
        {
            objectCache.Remove(key);
        }

        public bool CacheValid(string key)
        {
            if (objectCache.TryGetValue(key, out var result) && 
                result.Item1 > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, _cacheSeconds)))
                return true;
            return false;
        }
    }
}
