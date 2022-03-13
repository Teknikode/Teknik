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
                cacheDate = result.Item1;
                foundObject = (T)result.Item2;
            }
            else
            {
                foundObject = getObjectFunc(key);
            }

            if (foundObject != null)
                objectCache[key] = new Tuple<DateTime, object>(cacheDate, foundObject);

            return foundObject;
        }

        public void UpdateObject<T>(string key, T update)
        {
            var cacheDate = DateTime.UtcNow;
            if (objectCache.TryGetValue(key, out var result))
            {
                if (result.Item1 <= cacheDate.Subtract(new TimeSpan(0, 0, _cacheSeconds)))
                    DeleteObject(key);
                else
                    objectCache[key] = new Tuple<DateTime, object>(result.Item1, update);
            }
        }

        public void DeleteObject(string key)
        {
            objectCache.Remove(key);
        }
    }
}
