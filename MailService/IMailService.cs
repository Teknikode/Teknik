using System;

namespace Teknik.MailService
{
    public interface IMailService
    {
        bool AccountExists(string username);

        DateTime LastActive(string username);

        bool IsEnabled(string username);

        bool CreateAccount(string username, string password, long size);

        bool EditPassword(string username, string password);

        long GetMaxSize(string username);

        bool EditMaxSize(string username, long size);

        bool EditMaxEmailsPerDay(string username, int maxPerDay);

        bool EnableAccount(string username);

        bool DisableAccount(string username);

        bool DeleteAccount(string username);
    }
}
