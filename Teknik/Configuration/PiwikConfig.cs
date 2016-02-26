using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class PiwikConfig
    {
        public bool Enabled { get; set; }

        public string Url { get; set; }

        public int SiteId { get; set; }

        public PiwikConfig()
        {
            Enabled = false;
            Url = string.Empty;
            SiteId = 1;
        }
    }
}
