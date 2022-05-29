using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public class CacheEntry
    {
        public bool RollingExpiration { get; set; }
        public TimeSpan CacheTime { get; set; }
        public DateTime LastUpdate { get; set; }
        public object Data { get; set; }

        public CacheEntry(bool rollingExpiration, TimeSpan cacheTime, DateTime lastUpdate, object data)
        {
            RollingExpiration = rollingExpiration;
            CacheTime = cacheTime;
            LastUpdate = lastUpdate;
            Data = data;
        }
    }
}
