using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.MailService
{
    public abstract class MailService : IMailService
    {
        public abstract void CreateAccount(string username, string password, int size);

        public abstract bool AccountExists(string username);

        public abstract void Delete(string username);

        public abstract void Disable(string username);

        public abstract void EditActivity(string username, bool active);

        public abstract void EditMaxEmailsPerDay(string username, int maxPerDay);

        public abstract void EditMaxSize(string username, int size);

        public abstract void EditPassword(string username, string password);

        public abstract void Enable(string username);

        public abstract DateTime LastActive(string username);
    }
}
