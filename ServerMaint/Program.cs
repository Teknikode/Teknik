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

                        // Warns all the invalid accounts via email
                        if (options.WarnAccounts)
                        {
                            WarnInvalidAccounts(config, db);
                        }

                        // Cleans all inactive users
                        if (options.CleanUsers)
                        {
                            CleanAccounts(config, db, options.DaysBeforeDeletion);
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

                        // Generates a file for all of the invalid accounts
                        if (options.GenerateInvalid)
                        {
                            GenerateInvalidAccounts(config, db, options.InvalidFile);
                        }

                        // Generates a file for all of the accounts to be cleaned
                        if (options.GenerateCleaning)
                        {
                            GenerateCleaningList(config, db, options.CleaningFile, options.DaysBeforeDeletion);
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

        public static void WarnInvalidAccounts(Config config, TeknikEntities db)
        {

            Output(string.Format("[{0}] Started Warning of Invalid Accounts.", DateTime.Now));
            List<string> invalidAccounts = GetInvalidAccounts(config, db);

            foreach (string account in invalidAccounts)
            {
                // Let's send them an email :D
                string email = UserHelper.GetUserEmailAddress(config, account);
                
                SmtpClient client = new SmtpClient();
                client.Host = config.ContactConfig.Host;
                client.Port = config.ContactConfig.Port;
                client.EnableSsl = config.ContactConfig.SSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = true;
                client.Credentials = new NetworkCredential(config.ContactConfig.Username, config.ContactConfig.Password);
                client.Timeout = 5000;

                try
                {
                    MailMessage mail = new MailMessage(config.SupportEmail, email);
                    mail.Subject = "Invalid Account Notice";
                    mail.Body = string.Format(@"
The account {0} does not meet the requirements for a valid username.  

The username must meet the following requirements: {1}  
It must also be greater than or equal to {2} characters in length, and less than or equal to {3} characters in length.

This email is to let you know that this account will be deleted in {4} days ({5}) in order to comply with the username restrictions.  If you would like to keep your data, you should create a new account and transfer the data over to the new account.  

In order to make the process as easy as possible, you can reply to this email to ask for your current account to be renamed to another available account.  This would keep all your data intact, and just require you to change all references to your email/git/user to the new username.  If you wish to do this, please respond within {6} days ({7}) with the new username you would like to use.

Thank you for your continued use of Teknik!

- Teknik Administration", account, config.UserConfig.UsernameFilterLabel, config.UserConfig.MinUsernameLength, config.UserConfig.MaxUsernameLength, 30, DateTime.Now.AddDays(30).ToShortDateString(), 15, DateTime.Now.AddDays(15).ToShortDateString());
                    mail.BodyEncoding = UTF8Encoding.UTF8;
                    mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    Output(string.Format("[{0}] Unable to send email to {1}.  Exception: {2}", DateTime.Now, email, ex.Message));
                }
            }

            Output(string.Format("[{0}] Finished Warning of Invalid Accounts.  {1} Accounts Warned.", DateTime.Now, invalidAccounts.Count));
        }

        public static void CleanAccounts(Config config, TeknikEntities db, int maxDays)
        {
            Output(string.Format("[{0}] Started Cleaning of Inactive/Invalid Users.", DateTime.Now));
            List<string> invalidAccounts = GetInvalidAccounts(config, db);
            List<string> inactiveAccounts = GetInactiveAccounts(config, db, maxDays);

            // Delete invalid accounts
            foreach (string account in invalidAccounts)
            {
                UserHelper.DeleteAccount(db, config, UserHelper.GetUser(db, account));
            }

            if (invalidAccounts.Count > 0)
            {
                // Add to transparency report if any users were removed
                Takedown report = db.Takedowns.Create();
                report.Requester = TAKEDOWN_REPORTER;
                report.RequesterContact = config.SupportEmail;
                report.DateRequested = DateTime.Now;
                report.Reason = "Username Invalid";
                report.ActionTaken = string.Format("{0} Accounts Removed", invalidAccounts.Count);
                report.DateActionTaken = DateTime.Now;
                db.Takedowns.Add(report);
                db.SaveChanges();
            }

            // Delete inactive accounts
            foreach (string account in inactiveAccounts)
            {
                UserHelper.DeleteAccount(db, config, UserHelper.GetUser(db, account));
            }

            if (invalidAccounts.Count > 0)
            {
                // Add to transparency report if any users were removed
                Takedown report = db.Takedowns.Create();
                report.Requester = TAKEDOWN_REPORTER;
                report.RequesterContact = config.SupportEmail;
                report.DateRequested = DateTime.Now;
                report.Reason = "Account Inactive";
                report.ActionTaken = string.Format("{0} Accounts Removed", inactiveAccounts.Count);
                report.DateActionTaken = DateTime.Now;
                db.Takedowns.Add(report);
                db.SaveChanges();
            }

            Output(string.Format("[{0}] Finished Cleaning of Inactive/Invalid Users.  {1} Accounts Removed.", DateTime.Now, invalidAccounts.Count + inactiveAccounts.Count));
        }

        public static void CleanEmail(Config config, TeknikEntities db)
        {
            Output(string.Format("[{0}] Started Cleaning of Orphaned Email Accounts.", DateTime.Now));
            List<string> emails = GetOrphanedEmail(config, db);
            foreach (string email in emails)
            {
                // User doesn't exist, and it isn't reserved.  Let's nuke it.
                UserHelper.DeleteUserEmail(config, email);
            }

            if (emails.Count > 0)
            {
                // Add to transparency report if any users were removed
                Takedown report = db.Takedowns.Create();
                report.Requester = TAKEDOWN_REPORTER;
                report.RequesterContact = config.SupportEmail;
                report.DateRequested = DateTime.Now;
                report.Reason = "Orphaned Email Account";
                report.ActionTaken = string.Format("{0} Accounts Removed", emails.Count);
                report.DateActionTaken = DateTime.Now;
                db.Takedowns.Add(report);
                db.SaveChanges();
            }

            Output(string.Format("[{0}] Finished Cleaning of Orphaned Email Accounts.  {1} Accounts Removed.", DateTime.Now, emails.Count));
        }

        public static void CleanGit(Config config, TeknikEntities db)
        {
            Output(string.Format("[{0}] Started Cleaning of Orphaned Git Accounts.", DateTime.Now));
            List<string> gitAccounts = GetOrphanedGit(config, db);
            foreach (string account in gitAccounts)
            {
                // User doesn't exist, and it isn't reserved.  Let's nuke it.
                UserHelper.DeleteUserGit(config, account);
            }

            if (gitAccounts.Count > 0)
            {
                // Add to transparency report if any users were removed
                Takedown report = db.Takedowns.Create();
                report.Requester = TAKEDOWN_REPORTER;
                report.RequesterContact = config.SupportEmail;
                report.DateRequested = DateTime.Now;
                report.Reason = "Orphaned Git Account";
                report.ActionTaken = string.Format("{0} Accounts Removed", gitAccounts.Count);
                report.DateActionTaken = DateTime.Now;
                db.Takedowns.Add(report);
                db.SaveChanges();
            }

            Output(string.Format("[{0}] Finished Cleaning of Orphaned Git Accounts.  {1} Accounts Removed.", DateTime.Now, gitAccounts.Count));
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

        public static void GenerateInvalidAccounts(Config config, TeknikEntities db, string fileName)
        {
            Output(string.Format("[{0}] Started Generation of Invalid Account List.", DateTime.Now));
            List<string> invalidAccounts = GetInvalidAccounts(config, db);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Username,Last Activity,Creation Date,Last Website Activity,Last Email Activity,Last Git Activity");
            foreach (string account in invalidAccounts)
            {
                User user = UserHelper.GetUser(db, account);
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
            Output(string.Format("[{0}] Finished Generating Invalid Account List.", DateTime.Now));
        }

        public static void GenerateCleaningList(Config config, TeknikEntities db, string fileName, int maxDays)
        {
            Output(string.Format("[{0}] Started Generation of Accounts to Clean List.", DateTime.Now));
            List<string> invalidAccounts = GetInvalidAccounts(config, db);
            List<string> inactiveAccounts = GetInactiveAccounts(config, db, maxDays);
            List<string> emailAccounts = GetOrphanedEmail(config, db);
            List<string> gitAccounts = GetOrphanedGit(config, db);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Invalid Account Cleaning");
            sb.AppendLine("Username,Last Activity,Creation Date,Last Website Activity,Last Email Activity,Last Git Activity");
            foreach (string account in invalidAccounts)
            {
                User user = UserHelper.GetUser(db, account);
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                                user.Username,
                                UserHelper.GetLastAccountActivity(db, config, user).ToString("g"),
                                user.JoinDate.ToString("g"),
                                user.LastSeen.ToString("g"),
                                UserHelper.UserEmailLastActive(config, UserHelper.GetUserEmailAddress(config, user.Username)).ToString("g"),
                                UserHelper.UserGitLastActive(config, user.Username).ToString("g")));
            }

            sb.AppendLine();
            sb.AppendLine("Inactive Account Cleaning");
            sb.AppendLine("Username,Last Activity,Creation Date,Last Website Activity,Last Email Activity,Last Git Activity");
            foreach (string account in inactiveAccounts)
            {
                User user = UserHelper.GetUser(db, account);
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                                user.Username,
                                UserHelper.GetLastAccountActivity(db, config, user).ToString("g"),
                                user.JoinDate.ToString("g"),
                                user.LastSeen.ToString("g"),
                                UserHelper.UserEmailLastActive(config, UserHelper.GetUserEmailAddress(config, user.Username)).ToString("g"),
                                UserHelper.UserGitLastActive(config, user.Username).ToString("g")));
            }

            sb.AppendLine();
            sb.AppendLine("Orphaned Email Cleaning");
            sb.AppendLine("Email,Last Activity");
            foreach (string account in emailAccounts)
            {
                sb.AppendLine(string.Format("{0},{1}",
                                account,
                                UserHelper.UserEmailLastActive(config, account).ToString("g")));
            }

            sb.AppendLine();
            sb.AppendLine("Orphaned Git Cleaning");
            sb.AppendLine("Username,Last Activity");
            foreach (string account in gitAccounts)
            {
                sb.AppendLine(string.Format("{0},{1}",
                                account,
                                UserHelper.UserGitLastActive(config, account).ToString("g")));
            }

            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fileName, sb.ToString());
            Output(string.Format("[{0}] Finished Generating Accounts to Clean List.", DateTime.Now));
        }

        public static List<string> GetInvalidAccounts(Config config, TeknikEntities db)
        {
            List<string> foundUsers = new List<string>();
            List<User> curUsers = db.Users.ToList();
            foreach (User user in curUsers)
            {
                // If the username is reserved, let's add it to the list
                if (UserHelper.UsernameReserved(config, user.Username))
                {
                    foundUsers.Add(user.Username);
                    continue;
                }

                // If the username is invalid, let's add it to the list
                if (!UserHelper.ValidUsername(config, user.Username))
                {
                    foundUsers.Add(user.Username);
                    continue;
                }
            }
            return foundUsers;
        }

        public static List<string> GetInactiveAccounts(Config config, TeknikEntities db, int maxDays)
        {
            List<string> foundUsers = new List<string>();
            List<User> curUsers = db.Users.ToList();
            foreach (User user in curUsers)
            {
                // If the username is reserved, don't worry about it
                if (UserHelper.UsernameReserved(config, user.Username))
                {
                    continue;
                }

                #region Inactivity Finding
                DateTime lastActivity = UserHelper.GetLastAccountActivity(db, config, user);

                TimeSpan inactiveTime = DateTime.Now.Subtract(lastActivity);

                // If older than max days, check their current usage
                if (inactiveTime >= new TimeSpan(maxDays, 0, 0, 0, 0))
                {
                    // Check the user's usage of the service.
                    bool noData = true;

                    // Any blog comments?
                    var blogCom = db.BlogComments.Where(c => c.UserId == user.UserId);
                    noData &= !(blogCom != null && blogCom.Any());

                    // Any blog posts?
                    var blogPosts = db.BlogPosts.Where(p => p.Blog.UserId == user.UserId);
                    noData &= !(blogPosts != null && blogPosts.Any());

                    // Any podcast comments?
                    var podCom = db.PodcastComments.Where(p => p.UserId == user.UserId);
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
                        foundUsers.Add(user.Username);
                    }
                    continue;
                }
                #endregion
            }
            return foundUsers;
        }

        public static List<string> GetOrphanedEmail(Config config, TeknikEntities db)
        {
            List<string> foundEmail = new List<string>();
            if (config.EmailConfig.Enabled)
            {
                List<User> curUsers = db.Users.ToList();

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
                        foundEmail.Add(account.Address);
                    }
                }
            }
            return foundEmail;
        }

        public static List<string> GetOrphanedGit(Config config, TeknikEntities db)
        {
            List<string> foundGit = new List<string>();
            if (config.GitConfig.Enabled)
            {
                List<User> curUsers = db.Users.ToList();

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
                            foundGit.Add(account["username"].ToString());
                        }
                    }
                }
            }
            return foundGit;
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
