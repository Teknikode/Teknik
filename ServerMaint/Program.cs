using nClam;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Teknik.Areas.Transparency.Models;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Helpers;
using Teknik.Models;

namespace ServerMaint
{
    public class Program
    {
        private static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string virusFile = Path.Combine(currentPath, "virusLogs.txt");
        private static string errorFile = Path.Combine(currentPath, "errorLogs.txt");
        private static string configPath = currentPath;

        private const string TAKEDOWN_REPORTER = "Teknik Automated System";

        public static event Action<string> OutputEvent;

        public static int Main(string[] args)
        {
            try
            {
                ArgumentOptions options = new ArgumentOptions();
                var parser = new CommandLine.Parser(config => config.HelpWriter = Console.Out);
                if (parser.ParseArguments(args, options))
                {
                    if (!string.IsNullOrEmpty(options.Config))
                        configPath = options.Config;

                    if (Directory.Exists(configPath))
                    {
                        Config config = Config.Load(configPath);
                        TeknikEntities db = new TeknikEntities();

                        Output(string.Format("[{0}] Started Server Maintenance Process.", DateTime.Now));

                        // Scan all the uploads for viruses, and remove the bad ones
                        if (options.ScanUploads && config.UploadConfig.VirusScanEnable)
                        {
                            ScanUploads(config, db);
                        }

                        // Cleans all inactive users
                        if (options.CleanUsers)
                        {
                            CleanUsers(config, db, options.DaysBeforeDeletion);
                        }

                        // Cleans the email for unused accounts
                        if (options.CleanEmails)
                        {
                            CleanEmail(config, db);
                        }

                        // Cleans all the git accounts that are unused
                        if (options.CleanGit)
                        {
                            CleanGit(config, db);
                        }

                        // Generates a file for all of the user's last seen dates
                        if (options.GenerateLastSeen)
                        {
                            GenerateLastSeen(config, db, options.LastSeenFile);
                        }

                        Output(string.Format("[{0}] Finished Server Maintenance Process.", DateTime.Now));
                        return 0;
                    }
                    else
                    {
                        string msg = string.Format("[{0}] Config File does not exist.", DateTime.Now);
                        File.AppendAllLines(errorFile, new List<string> { msg });
                        Output(msg);
                    }
                }
                else
                {
                    Output(options.GetUsage());
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("[{0}] Exception: {1}", DateTime.Now, ex.GetFullMessage(true));
                File.AppendAllLines(errorFile, new List<string> { msg });
                Output(msg);
            }
            return -1;
        }

        public static void ScanUploads(Config config, TeknikEntities db)
        {
            Output(string.Format("[{0}] Started Virus Scan.", DateTime.Now));
            List<Upload> uploads = db.Uploads.ToList();

            // Initialize ClamAV
            ClamClient clam = new ClamClient(config.UploadConfig.ClamServer, config.UploadConfig.ClamPort);
            clam.MaxStreamSize = config.UploadConfig.MaxUploadSize;

            int totalCount = uploads.Count();
            int totalScans = 0;
            int totalClean = 0;
            int totalViruses = 0;
            foreach (Upload upload in uploads)
            {
                totalScans++;
                string subDir = upload.FileName[0].ToString();
                string filePath = Path.Combine(config.UploadConfig.UploadDirectory, subDir, upload.FileName);
                if (File.Exists(filePath))
                {
                    // Read in the file
                    byte[] data = File.ReadAllBytes(filePath);
                    // If the IV is set, and Key is set, then decrypt it
                    if (!string.IsNullOrEmpty(upload.Key) && !string.IsNullOrEmpty(upload.IV))
                    {
                        // Decrypt the data
                        data = AES.Decrypt(data, upload.Key, upload.IV);
                    }

                    // We have the data, let's scan it
                    ClamScanResult scanResult = clam.SendAndScanFile(data);

                    switch (scanResult.Result)
                    {
                        case ClamScanResults.Clean:
                            totalClean++;
                            string cleanMsg = string.Format("[{0}] Clean Scan: {1}/{2} Scanned | {3} - {4}", DateTime.Now, totalScans, totalCount, upload.Url, upload.FileName);
                            Output(cleanMsg);
                            break;
                        case ClamScanResults.VirusDetected:
                            totalViruses++;
                            string msg = string.Format("[{0}] Virus Detected: {1} - {2} - {3}", DateTime.Now, upload.Url, upload.FileName, scanResult.InfectedFiles.First().VirusName);
                            File.AppendAllLines(virusFile, new List<string> { msg });
                            Output(msg);
                            // Delete from the DB
                            db.Uploads.Remove(upload);
                            db.SaveChanges();

                            // Delete the File
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            break;
                        case ClamScanResults.Error:
                            string errorMsg = string.Format("[{0}] Scan Error: {1}", DateTime.Now, scanResult.RawResult);
                            File.AppendAllLines(errorFile, new List<string> { errorMsg });
                            Output(errorMsg);
                            break;
                        case ClamScanResults.Unknown:
                            string unkMsg = string.Format("[{0}] Unknown Scan Result: {1}", DateTime.Now, scanResult.RawResult);
                            File.AppendAllLines(errorFile, new List<string> { unkMsg });
                            Output(unkMsg);
                            break;
                    }
                }
            }

            if (totalViruses > 0)
            {
                // Add to transparency report if any were found
                Takedown report = db.Takedowns.Create();
                report.Requester = TAKEDOWN_REPORTER;
                report.RequesterContact = config.SupportEmail;
                report.DateRequested = DateTime.Now;
                report.Reason = "Malware Found";
                report.ActionTaken = string.Format("{0} Uploads removed", totalViruses);
                report.DateActionTaken = DateTime.Now;
                db.Takedowns.Add(report);
                db.SaveChanges();
            }

            Output(string.Format("Scanning Complete.  {0} Scanned | {1} Viruses Found | {2} Total Files", totalScans, totalViruses, totalCount));
        }

        public static void CleanUsers(Config config, TeknikEntities db, int maxDays)
        {
            int totalUsers = 0;

            Output(string.Format("[{0}] Started Cleaning of Inactive Users.", DateTime.Now));
            List<User> curUsers = db.Users.ToList();
            foreach (User user in curUsers)
            {
                // If the username is reserved, don't clean it
                if (UserHelper.UsernameReserved(config, user.Username))
                {
                    continue;
                }

                // If the username is invalid, let's clean the sucker, data and all
                if (!UserHelper.ValidUsername(config, user.Username))
                {
                    UserHelper.DeleteAccount(db, config, user);
                    continue;
                }

                #region Inactivity Cleaning
                DateTime lastActivity = UserHelper.GetLastAccountActivity(db, config, user);

                TimeSpan inactiveTime = DateTime.Now.Subtract(lastActivity);

                // If older than max days, check their current usage
                if (inactiveTime >= new TimeSpan(maxDays, 0, 0, 0, 0))
                {
                    // Check the user's usage of the service.
                    bool noData = true;

                    // Any blog comments?
                    var blogCom = db.BlogComments.Include("Users").Where(c => c.UserId == user.UserId);
                    noData &= !(blogCom != null && blogCom.Any());

                    // Any blog posts?
                    var blogPosts = db.BlogPosts.Include("Blog").Include("Blog.Users").Where(p => p.Blog.UserId == user.UserId);
                    noData &= !(blogPosts != null && blogPosts.Any());

                    // Any podcast comments?
                    var podCom = db.PodcastComments.Include("Users").Where(p => p.UserId == user.UserId);
                    noData &= !(podCom != null && podCom.Any());

                    // Any email?
                    if (config.EmailConfig.Enabled)
                    {
                        var app = new hMailServer.Application();
                        app.Connect();
                        app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);

                        try
                        {
                            var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                            var account = domain.Accounts.ItemByAddress[UserHelper.GetUserEmailAddress(config, user.Username)];
                            noData &= ((account.Messages.Count == 0) && ((int)account.Size == 0));
                        }
                        catch { }
                    }

                    // Any git repos?
                    if (config.GitConfig.Enabled)
                    {
                        string email = UserHelper.GetUserEmailAddress(config, user.Username);
                        // We need to check the actual git database
                        MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database);
                        string sql = @"SELECT * FROM gogs.repository
                                        LEFT JOIN gogs.action ON gogs.user.id = gogs.action.act_user_id
                                        WHERE gogs.user.login_name = {0}";
                        var results = mySQL.Query(sql, new object[] { email });

                        noData &= !(results != null && results.Any());
                    }

                    if (noData)
                    {
                        // They have no data, so safe to delete them.
                        UserHelper.DeleteAccount(db, config, UserHelper.GetUser(db, user.Username));
                        totalUsers++;
                    }
                    continue;
                }
                #endregion
            }

            if (totalUsers > 0)
            {
                // Add to transparency report if any users were removed
                Takedown report = db.Takedowns.Create();
                report.Requester = TAKEDOWN_REPORTER;
                report.RequesterContact = config.SupportEmail;
                report.DateRequested = DateTime.Now;
                report.Reason = "User Inactive";
                report.ActionTaken = string.Format("{0} Users Removed", totalUsers);
                report.DateActionTaken = DateTime.Now;
                db.Takedowns.Add(report);
                db.SaveChanges();
            }

            Output(string.Format("[{0}] Finished Cleaning of Inactive Users.  {1} Users Removed.", DateTime.Now, totalUsers));
        }

        public static void CleanEmail(Config config, TeknikEntities db)
        {
            if (config.EmailConfig.Enabled)
            {
                Output(string.Format("[{0}] Started Cleaning of Orphaned Email Accounts.", DateTime.Now));
                List<User> curUsers = db.Users.ToList();
                int totalAccounts = 0;

                // Connect to hmailserver COM
                var app = new hMailServer.Application();
                app.Connect();
                app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);

                var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                var accounts = domain.Accounts;
                for (int i = 0; i < accounts.Count; i++)
                {
                    var account = accounts[i];

                    bool userExists = curUsers.Exists(u => UserHelper.GetUserEmailAddress(config, u.Username) == account.Address);
                    bool isReserved = UserHelper.GetReservedUsernames(config).Exists(r => UserHelper.GetUserEmailAddress(config, r).ToLower() == account.Address.ToLower());
                    if (!userExists && !isReserved)
                    {
                        // User doesn't exist, and it isn't reserved.  Let's nuke it.
                        UserHelper.DeleteUserEmail(config, account.Address);
                        totalAccounts++;
                    }
                }

                if (totalAccounts > 0)
                {
                    // Add to transparency report if any users were removed
                    Takedown report = db.Takedowns.Create();
                    report.Requester = TAKEDOWN_REPORTER;
                    report.RequesterContact = config.SupportEmail;
                    report.DateRequested = DateTime.Now;
                    report.Reason = "Orphaned Email Account";
                    report.ActionTaken = string.Format("{0} Accounts Removed", totalAccounts);
                    report.DateActionTaken = DateTime.Now;
                    db.Takedowns.Add(report);
                    db.SaveChanges();
                }

                Output(string.Format("[{0}] Finished Cleaning of Orphaned Email Accounts.  {1} Accounts Removed.", DateTime.Now, totalAccounts));
            }
        }

        public static void CleanGit(Config config, TeknikEntities db)
        {
            if (config.GitConfig.Enabled)
            {
                Output(string.Format("[{0}] Started Cleaning of Orphaned Git Accounts.", DateTime.Now));
                List<User> curUsers = db.Users.ToList();
                int totalAccounts = 0;

                // We need to check the actual git database
                MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database);
                string sql = @"SELECT gogs.user.login_name AS login_name, gogs.user.lower_name AS username FROM gogs.user";
                var results = mySQL.Query(sql);

                if (results != null && results.Any())
                {
                    foreach (var account in results)
                    {
                        bool userExists = curUsers.Exists(u => UserHelper.GetUserEmailAddress(config, u.Username).ToLower() == account["login_name"].ToString().ToLower());
                        bool isReserved = UserHelper.GetReservedUsernames(config).Exists(r => UserHelper.GetUserEmailAddress(config, r) == account["login_name"].ToString().ToLower());
                        if (!userExists && !isReserved)
                        {
                            UserHelper.DeleteUserGit(config, account["username"].ToString());
                            totalAccounts++;
                        }
                    }
                }

                if (totalAccounts > 0)
                {
                    // Add to transparency report if any users were removed
                    Takedown report = db.Takedowns.Create();
                    report.Requester = TAKEDOWN_REPORTER;
                    report.RequesterContact = config.SupportEmail;
                    report.DateRequested = DateTime.Now;
                    report.Reason = "Orphaned Git Account";
                    report.ActionTaken = string.Format("{0} Accounts Removed", totalAccounts);
                    report.DateActionTaken = DateTime.Now;
                    db.Takedowns.Add(report);
                    db.SaveChanges();
                }

                Output(string.Format("[{0}] Finished Cleaning of Orphaned Git Accounts.  {1} Accounts Removed.", DateTime.Now, totalAccounts));
            }
        }

        public static void GenerateLastSeen(Config config, TeknikEntities db, string fileName)
        {
            Output(string.Format("[{0}] Started Generation of Last Activity List.", DateTime.Now));
            List<User> curUsers = db.Users.ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Username,Last Activity,Creation Date,Last Website Activity,Last Email Activity,Last Git Activity");
            foreach (User user in curUsers)
            {
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                                user.Username,
                                UserHelper.GetLastAccountActivity(db, config, user).ToString("g"),
                                user.JoinDate.ToString("g"),
                                user.LastSeen.ToString("g"),
                                UserHelper.UserEmailLastActive(config, UserHelper.GetUserEmailAddress(config, user.Username)).ToString("g"),
                                UserHelper.UserGitLastActive(config, user.Username).ToString("g")));
            }
            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fileName, sb.ToString());
            Output(string.Format("[{0}] Finished Generating Last Activity List.", DateTime.Now));
        }

        public static void Output(string message)
        {
            Console.WriteLine(message);
            if (OutputEvent != null)
            {
                OutputEvent(message);
            }
        }
    }
}
