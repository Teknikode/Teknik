using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class LoggingConfig
    {
        public bool Enabled { get; set; }

        public string OutputDirectory { get; set; }

        public string LogLevel { get; set; }

        public bool RotateLogs { get; set; }

        public long MaxSize { get; set; }

        public int MaxCount { get; set; }

        public LoggingConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Enabled = true;
            OutputDirectory = string.Empty;
            LogLevel = "Info";
            RotateLogs = false;
            MaxSize = -1;
            MaxCount = -1;
        }
    }
}