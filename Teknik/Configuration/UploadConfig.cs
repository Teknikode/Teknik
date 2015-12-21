using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class UploadConfig
    {
        // Max upload size in bytes
        public int MaxUploadSize { get; set; }

        public UploadConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            MaxUploadSize = 100000000;
        }
    }
}