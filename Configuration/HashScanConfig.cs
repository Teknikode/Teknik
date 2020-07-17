using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Configuration
{
    public class HashScanConfig
    {
        public bool Enabled { get; set; }
        public string Endpoint { get; set; }
        public bool Authenticate { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public HashScanConfig()
        {
            Enabled = false;
            Endpoint = string.Empty;
            Authenticate = false;
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}
