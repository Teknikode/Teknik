using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.MailService
{
    public class HMailService : IMailService
    {
        private readonly hMailServer.Application _App;
        private readonly ILogger _logger;

        private string _Host { get; set; }
        private string _Username { get; set; }
        private string _Password { get; set; }
        private string _Domain { get; set; }

        private string _CounterServer { get; set; }
        private string _CounterDatabase { get; set; }
        private string _CounterUsername { get; set; }
        private string _CounterPassword { get; set; }
        private int _CounterPort { get; set; }

        public HMailService(string host, 
                            string username, 
                            string password, 
                            string domain, 
                            string counterServer, 
                            string counterDatabase, 
                            string counterUsername, 
                            string counterPassword, 
                            int counterPort,
                            ILogger logger)
        {
            _Host = host;
            _Username = username;
            _Password = password;
            _Domain = domain;

            _CounterServer = counterServer;
            _CounterDatabase = counterDatabase;
            _CounterUsername = counterUsername;
            _CounterPassword = counterPassword;
            _CounterPort = counterPort;

            _logger = logger;
            _App = InitApp();
        }

        public bool CreateAccount(string username, string password, long size)
        {
            var domain = _App.Domains.ItemByName[_Domain];
            var newAccount = domain.Accounts.Add();
            newAccount.Address = username;
            newAccount.Password = password;
            newAccount.Active = true;
            newAccount.MaxSize = (int)(size / 1000000);

            newAccount.Save();
            return true;
        }

        public bool AccountExists(string username)
        {
            var account = GetAccount(username);
            return account != null;
        }

        public bool DeleteAccount(string username)
        {
            var account = GetAccount(username);
            if (account != null)
            {
                account.Delete();
                return true;
            }
            return false;
        }

        public bool EnableAccount(string username)
        {
            return EditActivity(username, true);
        }

        public bool DisableAccount(string username)
        {
            return EditActivity(username, false);
        }

        public bool EditActivity(string username, bool active)
        {
            var account = GetAccount(username);
            if (account != null)
            {
                account.Active = active;
                account.Save();
                return true;
            }
            return false;
        }

        public bool EditMaxEmailsPerDay(string username, int maxPerDay)
        {
            //We need to check the actual git database
            MysqlDatabase mySQL = new MysqlDatabase(_CounterServer, _CounterDatabase, _CounterUsername, _CounterPassword, _CounterPort);
            string sql = @"INSERT INTO mailcounter.counts (qname, lastdate, qlimit, count) VALUES ({1}, NOW(), {0}, 0)
                                    ON DUPLICATE KEY UPDATE qlimit = {0}";
            mySQL.Execute(sql, new object[] { maxPerDay, username });
            return true;
        }

        public long GetMaxSize(string username)
        {
            var account = GetAccount(username);
            return account?.MaxSize ?? 0;
        }

        public bool EditMaxSize(string username, long size)
        {
            var account = GetAccount(username);
            if (account != null)
            {
                account.MaxSize = (int)(size / 1000000);
                account.Save();
                return true;
            }
            return false;
        }

        public bool EditPassword(string username, string password)
        {
            var account = GetAccount(username);
            if (account != null)
            {
                account.Password = password;
                account.Save();
                return true;
            }
            return false;
        }

        public DateTime LastActive(string username)
        {
            var account = GetAccount(username);
            return (DateTime)(account?.LastLogonTime ?? DateTime.MinValue);
        }

        private hMailServer.Application InitApp()
        {
            var app = new hMailServer.Application();
            app.Connect();
            app.Authenticate(_Username, _Password);

            return app;
        }

        private hMailServer.Account GetAccount(string username)
        {
            try
            {
                var domain = _App.Domains.ItemByName[_Domain];
                return domain.Accounts.ItemByAddress[username];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred getting Email Account");
            }
            return null;
        }

        public bool IsEnabled(string username)
        {
            var account = GetAccount(username);
            return account?.Active ?? false;
        }
    }
}
