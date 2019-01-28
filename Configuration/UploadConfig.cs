using System.Collections.Generic;
using System.IO;

namespace Teknik.Configuration
{
    public class UploadConfig
    {
        public bool UploadEnabled { get; set; }
        public bool DownloadEnabled { get; set; }
        // Max upload size in bytes
        public long MaxUploadSize { get; set; }
        // Max Upload Size for basic users
        public long MaxUploadSizeBasic { get; set; }
        // Max Upload Size for premium users
        public long MaxUploadSizePremium { get; set; }
        // Gets the maximum download size before they are forced to the download page
        public long MaxDownloadSize { get; set; }
        // Location of the upload directory
        public string UploadDirectory { get; set; }
        // File Extension for saved files
        public string FileExtension { get; set; }
        public int UrlLength { get; set; }
        public int DeleteKeyLength { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public bool IncludeExtension { get; set; }
        // The size of the chunk that the file will be encrypted/decrypted in (bytes)
        public int ChunkSize { get; set; }
        // Virus Scanning Settings
        public bool VirusScanEnable { get; set; }
        public string ClamServer { get; set; }
        public int ClamPort { get; set; }
        // Content Type Restrictions
        public List<string> RestrictedContentTypes { get; set; }
        public List<string> RestrictedExtensions { get; set; }

        public UploadConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            UploadEnabled = true;
            DownloadEnabled = true;
            MaxUploadSize = 100000000;
            MaxUploadSizeBasic = 100000000;
            MaxUploadSizePremium = 100000000;
            MaxDownloadSize = 100000000;
            UploadDirectory = Directory.GetCurrentDirectory();
            FileExtension = "enc";
            UrlLength = 5;
            DeleteKeyLength = 24;
            KeySize = 256;
            BlockSize = 128;
            IncludeExtension = true;
            ChunkSize = 1024;
            VirusScanEnable = false;
            ClamServer = "localhost";
            ClamPort = 3310;
            RestrictedContentTypes = new List<string>();
            RestrictedExtensions = new List<string>();
        }
    }
}
