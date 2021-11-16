using System;

namespace Teknik.MailService
{
    public interface IMailService
    {
        bool AccountExists(string username);

        DateTime LastActive(string username);

        bool Enabled(string username);

        void CreateAccount(string username, string password, long size);

        void EditPassword(string username, string password);

        void EditMaxSize(string username, long size);

        void EditMaxEmailsPerDay(string username, int maxPerDay);

        void EnableAccount(string username);

        void DisableAccount(string username);

        void DeleteAccount(string username);
    }
}
