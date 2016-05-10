using nClam;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Teknik.Areas.Upload.Models;
using Teknik.Configuration;
using Teknik.Helpers;
using Teknik.Models;

namespace ServerMaint
{
    public class Program
    {
        private static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string parentPath = Directory.GetParent(currentPath).FullName;
        private static string virusFile = Path.Combine(currentPath, "virusLogs.txt");
        private static string errorFile = Path.Combine(currentPath, "errorLogs.txt");
        private static string configPath = Path.Combine(parentPath, "App_Data");

        public static event Action<string> OutputEvent;

        public static void Main(string[] args)
        {

            // Let's clean some stuff!!
            try
            {
                Config config = Config.Load(configPath);
                TeknikEntities db = new TeknikEntities();

                // Scan all the uploads for viruses, and remove the bad ones
                if (config.UploadConfig.VirusScanEnable)
                {
                    ScanUploads(config, db);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("[{0}] Exception: {1}", DateTime.Now, ex.Message);
                File.AppendAllLines(errorFile, new List<string> { msg });
                Output(msg);
            }
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
                            //// Delete from the DB
                            //db.Uploads.Remove(upload);
                            //db.SaveChanges();

                            //// Delete the File
                            //if (File.Exists(filePath))
                            //{
                            //    File.Delete(filePath);
                            //}
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

            Output(string.Format("Scanning Complete.  {0} Scanned | {1} Viruses Found | {2} Total Files", totalScans, totalViruses, totalCount));
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
