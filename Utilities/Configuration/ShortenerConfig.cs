using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class ShortenerConfig
    {
        public string ShortenerHost { get; set; }

        public int UrlLength { get; set; }

        public ShortenerConfig()
        {
            ShortenerHost = string.Empty;
            UrlLength = 4;
        }
    }
}