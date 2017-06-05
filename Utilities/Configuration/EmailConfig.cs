using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class EmailConfig
    {
        public bool Enabled { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public string MailHost { get; set; }

        public int MaxSize { get; set; }

        public DatabaseConfig CounterDatabase { get; set; }

        public EmailConfig()
        {
            Enabled = true;
            Username = string.Empty;
            Password = string.Empty;
            Domain = string.Empty;
            MailHost = string.Empty;
            MaxSize = 1000;
            CounterDatabase = new DatabaseConfig();
        }
    }
}