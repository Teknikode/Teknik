using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class EmailAccount
    {
        public string EmailAddress { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }

        public EmailAccount()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            EmailAddress = string.Empty;
            Host = string.Empty;
            Port = 25;
            Username = string.Empty;
            Password = string.Empty;
            SSL = false;
        }
    }
}
