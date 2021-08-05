using CommandLine;
using Microsoft.EntityFrameworkCore;
using nClam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Teknik.Areas.Paste.Models;
using Teknik.Areas.Stats.Models;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;

namespace Teknik.ServiceWorker
{
    public class Program
    {
        private static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string virusFile = Path.Combine(currentPath, "virusLogs.txt");
        private static string errorFile = Path.Combine(currentPath, "errorLogs.txt");
        private static string orphansFile = Path.Combine(currentPath, "orphanedFiles.txt");
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
                        optionsBuilder.UseSqlServer(config.DbConnection);

                        using (TeknikEntities db = new TeknikEntities(optionsBuilder.Options))
                        {
                            // Scan all the uploads for viruses, and remove the bad ones
                            if (options.ScanUploads && config.UploadConfig.ClamConfig.Enabled)
                            {
                                ScanUploads(config, db);
                            }

                            // Runs the migration
                            if (options.Migrate)
                            {
                                // Run the overall migration calls
                                TeknikMigration.RunMigration(db, config);
                            }

                            if (options.Expire)
                            {
                                ProcessExpirations(config, db);
                            }

                            if (options.Clean)
                            {
                                CleanStorage(config, db);
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

            int maxConcurrency = 100;
            int totalCount = uploads.Count();
            int totalScans = 0;
            int currentScan = 0;
            int totalViruses = 0;

            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
            {
                List<Task> runningTasks = new List<Task>();
                foreach (Upload upload in uploads)
                {
                    concurrencySemaphore.Wait();

                    currentScan++;

                    Task scanTask = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            var virusDetected = await ScanUpload(config, db, upload, totalCount, currentScan);
                            if (virusDetected)
                                totalViruses++;
                            totalScans++;
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = string.Format("[{0}] Scan Error: {1}", DateTime.Now, ex.GetFullMessage(true, true));
                            File.AppendAllLines(errorFile, new List<string> { errorMsg });
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                        }
                    });
                    if (scanTask != null)
                    {
                        runningTasks.Add(scanTask);
                    }
                }
                Task.WaitAll(runningTasks.ToArray());
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
                        ClamClient clam = new ClamClient(config.UploadConfig.ClamConfig.Server, config.UploadConfig.ClamConfig.Port);
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
                                    DeleteFile(filePath);

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

        public static void ProcessExpirations(Config config, TeknikEntities db)
        {
            Output(string.Format("[{0}] Starting processing expirations.", DateTime.Now));

            var curDate = DateTime.Now;

            // Process uploads
            List<Upload> uploads = db.Uploads.Where(u => u.ExpireDate != null && u.ExpireDate < curDate).ToList();

            foreach (Upload upload in uploads)
            {
                string subDir = upload.FileName[0].ToString();
                string filePath = Path.Combine(config.UploadConfig.UploadDirectory, subDir, upload.FileName);

                // Delete the File
                DeleteFile(filePath);
            }
            db.RemoveRange(uploads);
            db.SaveChanges();

            // Process Pastes
            List<Paste> pastes = db.Pastes.Where(p => p.ExpireDate != null && p.ExpireDate < curDate).ToList();

            foreach (Paste paste in pastes)
            {
                string subDir = paste.FileName[0].ToString();
                string filePath = Path.Combine(config.PasteConfig.PasteDirectory, subDir, paste.FileName);

                // Delete the File
                DeleteFile(filePath);
            }
            db.RemoveRange(pastes);
            db.SaveChanges();
        }

        public static void CleanStorage(Config config, TeknikEntities db)
        {
            // Process upload data
            Output(string.Format("[{0}] Starting processing upload storage cleaning.", DateTime.Now));
            CleanUploadFiles(config, db);

            // Process paste data
            Output(string.Format("[{0}] Starting processing upload storage cleaning.", DateTime.Now));
            CleanPasteFiles(config, db);
        }

        public static void CleanUploadFiles(Config config, TeknikEntities db)
        {
            List<string> uploads = db.Uploads.Where(u => !string.IsNullOrEmpty(u.FileName)).Select(u => Path.Combine(config.UploadConfig.UploadDirectory, u.FileName[0].ToString(), u.FileName)).Select(u => u.ToLower()).ToList();
            List<string> files = Directory.GetFiles(config.UploadConfig.UploadDirectory, "*.*", SearchOption.AllDirectories).Select(f => f.ToLower()).ToList();
            var orphans = files.Except(uploads);
            File.AppendAllLines(orphansFile, orphans);
            foreach (var orphan in orphans)
            {
                DeleteFile(orphan);
            }
        }

        public static void CleanPasteFiles(Config config, TeknikEntities db)
        {
            List<string> pastes = db.Pastes.Where(p => !string.IsNullOrEmpty(p.FileName)).Select(p => Path.Combine(config.PasteConfig.PasteDirectory, p.FileName[0].ToString(), p.FileName)).Select(p => p.ToLower()).ToList();
            List<string> files = Directory.GetFiles(config.PasteConfig.PasteDirectory, "*.*", SearchOption.AllDirectories).Select(f => f.ToLower()).ToList();
            var orphans = files.Except(pastes);
            File.AppendAllLines(orphansFile, orphans);
            foreach (var orphan in orphans)
            {
                DeleteFile(orphan);
            }
        }

        public static void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Output(string.Format("[{0}] Unable to delete file: {1} | {2}", DateTime.Now, filePath, ex.ToString()));
            }
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
