using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class StatusConfig
    {
        public bool Enabled { get; set; }

        public bool ShowWebStatus { get; set; }

        public bool ShowDatabaseStatus { get; set; }

        public bool ShowNetworkStatus { get; set; }

        public string DatabaseProcessName { get; set; }

        public string NetworkInterface { get; set; }

        public long TotalMemory { get; set; }

        public StatusConfig()
        {
            Enabled = false;
            ShowWebStatus = false;
            ShowDatabaseStatus = false;
            ShowNetworkStatus = false;
            DatabaseProcessName = string.Empty;
            NetworkInterface = string.Empty;
            TotalMemory = 0;
        }
    }
}
