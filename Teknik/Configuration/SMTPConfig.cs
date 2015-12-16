using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Teknik
{
    public class SMTPConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }

        public SMTPConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Host = string.Empty;
            Port = 25;
            Username = string.Empty;
            Password = string.Empty;
            SSL = false;
        }
    }
}
