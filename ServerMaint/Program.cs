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

                        // Scan all the uploads for viruses, and remove the bad ones
                        if (options.ScanUploads && config.UploadConfig.VirusScanEnable)
                        {
                            ScanUploads(config, db);
                        }

                        // Cleans all inactive users
                        if (options.CleanUsers)
                        {
                            CleanUsers(config, db, options.DaysBeforeDeletion, options.EmailsToSend);
                        }
                        Output(string.Format("[{0}] Finished Server Maintainence Process.", DateTime.Now));
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
                string msg = string.Format("[{0}] Exception: {1}", DateTime.Now, ex.Message);
                File.AppendAllLines(errorFile, new List<string> { msg });
                Output(msg);
            }
            return -1;
        }

        public static void ScanUploads(Config config, TeknikEntities db)
        {
            List<Upload> uploads = db.Uploads.ToList();

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
                    ClamClient clam = new ClamClient(config.UploadConfig.ClamServer, config.UploadConfig.ClamPort);
                    clam.MaxStreamSize = config.UploadConfig.MaxUploadSize;
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

            Output(string.Format("Scanning Complete.  {0} Scanned | {1} Viruses Found | {2} Total Files", totalScans, totalViruses, totalCount));
        }

        public static void CleanUsers(Config config, TeknikEntities db, int maxDays, int numEmails)
        {
            int totalUsers = 0;

            List<User> curUsers = db.Users.ToList();
            foreach (User user in curUsers)
            {
                DateTime lastActivity = UserHelper.GetLastActivity(db, config, user);

                TimeSpan inactiveTime = DateTime.Now.Subtract(lastActivity);

                // If older than max days, delete
                if (inactiveTime >= new TimeSpan(maxDays, 0, 0, 0, 0))
                {
                    UserHelper.DeleteUser(db, config, user);
                    continue;
                }

                // Otherwise, send an email if they are within +-1 day of email days
                int daysForEmail = (int)Math.Floor((double)(maxDays / (numEmails + 1)));
                for (int i = daysForEmail; i < maxDays; i = i + daysForEmail)
                {
                    if (inactiveTime.Days == daysForEmail)
                    {
                        string email = string.Format("{0}@{1}", user.Username, config.EmailConfig.Domain);
                        // Let's send them an email
                        SmtpClient client = new SmtpClient();
                        client.Host = config.ContactConfig.Host;
                        client.Port = config.ContactConfig.Port;
                        client.EnableSsl = config.ContactConfig.SSL;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = true;
                        client.Credentials = new NetworkCredential(config.ContactConfig.Username, config.ContactConfig.Password);
                        client.Timeout = 5000;

                        MailMessage mail = new MailMessage(config.SupportEmail, email);
                        mail.Subject = "Inactive Account Notice";
                        mail.Body = string.Format(@"
The account {0} has not had any activity for {1} days.  After {2} days of inactivity, this account will be deleted permanently.  

In order to avoid this, login into your email, or teknik website.

Thank you for your use of Teknik and I hope you decide to come back.

- Teknik Administration", user.Username, inactiveTime.Days, maxDays);
                        mail.BodyEncoding = UTF8Encoding.UTF8;
                        mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                        client.Send(mail);
                        break;
                    }
                }
            }

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
