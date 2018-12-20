using CommandLine;
using Microsoft.EntityFrameworkCore;
using nClam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Stats.Models;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;

namespace ServiceWorker
{
    public class Program
    {
        private static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string virusFile = Path.Combine(currentPath, "virusLogs.txt");
        private static string errorFile = Path.Combine(currentPath, "errorLogs.txt");
        private static string configPath = currentPath;

        private const string TAKEDOWN_REPORTER = "Teknik Automated System";

        private static readonly object dbLock = new object();
        private static readonly object scanStatsLock = new object();

        public static event Action<string> OutputEvent;

        public static int Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<ArgumentOptions>(args).WithParsed(options =>
                {
                    if (!string.IsNullOrEmpty(options.Config))
                        configPath = options.Config;

                    if (Directory.Exists(configPath))
                    {
                        Config config = Config.Load(configPath);
                        Output(string.Format("[{0}] Started Server Maintenance Process.", DateTime.Now));

                        var optionsBuilder = new DbContextOptionsBuilder<TeknikEntities>();
                        optionsBuilder.UseSqlServer("Data Source=blog.db");

                        using (TeknikEntities db = new TeknikEntities(optionsBuilder.Options))
                        {
                            // Scan all the uploads for viruses, and remove the bad ones
                            if (options.ScanUploads && config.UploadConfig.VirusScanEnable)
                            {
                                ScanUploads(config, db);
                            }
                        }

                        Output(string.Format("[{0}] Finished Server Maintenance Process.", DateTime.Now));
                    }
                    else
                    {
                        string msg = string.Format("[{0}] Config File does not exist.", DateTime.Now);
                        File.AppendAllLines(errorFile, new List<string> { msg });
                        Output(msg);
                    }
                });
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

            int totalCount = uploads.Count();
            int totalScans = 0;
            int totalViruses = 0;
            List<Task> runningTasks = new List<Task>();
            foreach (Upload upload in uploads)
            {
                int currentScan = totalScans++;
                Task scanTask = Task.Factory.StartNew(async () =>
                {
                    var virusDetected = await ScanUpload(config, db, upload, totalCount, currentScan);
                    if (virusDetected)
                        totalViruses++;
                });
                if (scanTask != null)
                {
                    runningTasks.Add(scanTask);
                }
            }
            bool running = true;

            while (running)
            {
                running = runningTasks.Exists(s => s != null && !s.IsCompleted && !s.IsCanceled && !s.IsFaulted);
            }

            Output(string.Format("Scanning Complete.  {0} Scanned | {1} Viruses Found | {2} Total Files", totalScans, totalViruses, totalCount));
        }

        private static async Task<bool> ScanUpload(Config config, TeknikEntities db, Upload upload, int totalCount, int currentCount)
        {
            bool virusDetected = false;
            string subDir = upload.FileName[0].ToString();
            string filePath = Path.Combine(config.UploadConfig.UploadDirectory, subDir, upload.FileName);
            if (File.Exists(filePath))
            {
                // If the IV is set, and Key is set, then scan it
                if (!string.IsNullOrEmpty(upload.Key) && !string.IsNullOrEmpty(upload.IV))
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(upload.Key);
                    byte[] ivBytes = Encoding.UTF8.GetBytes(upload.IV);


                    long maxUploadSize = config.UploadConfig.MaxUploadSize;
                    if (upload.User != null)
                    {
                        maxUploadSize = config.UploadConfig.MaxUploadSizeBasic;
                        IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(config, upload.User.Username);
                        if (userInfo.AccountType == AccountType.Premium)
                        {
                            maxUploadSize = config.UploadConfig.MaxUploadSizePremium;
                        }
                    }

                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (AesCounterStream aesStream = new AesCounterStream(fs, false, keyBytes, ivBytes))
                    {
                        ClamClient clam = new ClamClient(config.UploadConfig.ClamServer, config.UploadConfig.ClamPort);
                        clam.MaxStreamSize = maxUploadSize;
                        ClamScanResult scanResult = await clam.SendAndScanFileAsync(fs);

                        switch (scanResult.Result)
                        {
                            case ClamScanResults.Clean:
                                string cleanMsg = string.Format("[{0}] Clean Scan: {1}/{2} Scanned | {3} - {4}", DateTime.Now, currentCount, totalCount, upload.Url, upload.FileName);
                                Output(cleanMsg);
                                break;
                            case ClamScanResults.VirusDetected:
                                string msg = string.Format("[{0}] Virus Detected: {1} - {2} - {3}", DateTime.Now, upload.Url, upload.FileName, scanResult.InfectedFiles.First().VirusName);
                                Output(msg);
                                lock (scanStatsLock)
                                {
                                    virusDetected = true;
                                    File.AppendAllLines(virusFile, new List<string> { msg });
                                }

                                lock (dbLock)
                                {
                                    string urlName = upload.Url;
                                    // Delete from the DB
                                    db.Uploads.Remove(upload);

                                    // Delete the File
                                    if (File.Exists(filePath))
                                    {
                                        File.Delete(filePath);
                                    }

                                    // Add to transparency report if any were found
                                    Takedown report = new Takedown();
                                    report.Requester = TAKEDOWN_REPORTER;
                                    report.RequesterContact = config.SupportEmail;
                                    report.DateRequested = DateTime.Now;
                                    report.Reason = "Malware Found";
                                    report.ActionTaken = string.Format("Upload removed: {0}", urlName);
                                    report.DateActionTaken = DateTime.Now;
                                    db.Takedowns.Add(report);

                                    // Save Changes
                                    db.SaveChanges();
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
            }
            return virusDetected;
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
