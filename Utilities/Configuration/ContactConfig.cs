using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Teknik.Configuration
{
    public class ContactConfig
    {
        public bool Enabled { get; set; }
        public EmailAccount EmailAccount { get; set; }

        public ContactConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Enabled = true;
            EmailAccount = new EmailAccount();
        }
    }
}
