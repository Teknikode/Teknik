using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class VaultConfig
    {
        public bool Enabled { get; set; }

        public int UrlLength { get; set; }

        public VaultConfig()
        {
            Enabled = true;
            UrlLength = 5;
        }
    }
}