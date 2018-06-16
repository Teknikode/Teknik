using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.GitService
{
    public interface IGitService
    {
        bool AccountExists(string username);

        DateTime LastActive(string username);

        void CreateAccount(string username, string email, string password);

        void EditPassword(string username, string email, string password);

        void EnableAccount(string username, string email);

        void DisableAccount(string username, string email);

        void DeleteAccount(string username);
    }
}
