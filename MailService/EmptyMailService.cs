using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.MailService
{
    public class EmptyMailService : IMailService
    {
        public bool AccountExists(string username)
        {
            throw new NotImplementedException();
        }

        public bool CreateAccount(string username, string password, long size)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAccount(string username)
        {
            throw new NotImplementedException();
        }

        public bool DisableAccount(string username)
        {
            throw new NotImplementedException();
        }

        public bool EditMaxEmailsPerDay(string username, int maxPerDay)
        {
            throw new NotImplementedException();
        }

        public bool EditMaxSize(string username, long size)
        {
            throw new NotImplementedException();
        }

        public bool EditPassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool EnableAccount(string username)
        {
            throw new NotImplementedException();
        }

        public long GetMaxSize(string username)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(string username)
        {
            throw new NotImplementedException();
        }

        public DateTime LastActive(string username)
        {
            throw new NotImplementedException();
        }
    }
}
