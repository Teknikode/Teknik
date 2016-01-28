using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class EmailConfig
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public string MailHost { get; set; }

        public int MaxSize { get; set; }

        public EmailConfig()
        {
            Username = string.Empty;
            Password = string.Empty;
            Domain = string.Empty;
            MailHost = string.Empty;
            MaxSize = 1000;
        }
    }
}