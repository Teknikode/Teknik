using System;

namespace Teknik.MailService
{
    public interface IMailService
    {
        bool AccountExists(string username);

        DateTime LastActive(string username);

        void CreateAccount(string username, string password, int size);

        void EditActivity(string username, bool active);

        void EditPassword(string username, string password);

        void EditMaxSize(string username, int size);

        void EditMaxEmailsPerDay(string username, int maxPerDay);

        void Enable(string username);

        void Disable(string username);

        void Delete(string username);
    }
}
