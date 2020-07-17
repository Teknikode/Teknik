using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Configuration
{
    public class ClamConfig
    {
        // Virus Scanning Settings
        public bool Enabled { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }

        public ClamConfig()
        {
            Enabled = false;
            Server = "localhost";
            Port = 3310;
        }
    }
}
