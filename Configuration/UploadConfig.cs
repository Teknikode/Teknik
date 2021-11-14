using System.Collections.Generic;
using System.IO;

namespace Teknik.Configuration
{
    public class UploadConfig
    {
        public bool UploadEnabled { get; set; }
        public bool DownloadEnabled { get; set; }
        // Max upload size in bytes for free/anonymous users
        public long MaxUploadFileSize { get; set; }
        // Maximum file size before they are forced to the download page
        public long MaxDownloadFileSize { get; set; }
        // Maximum storage for free users
        public long MaxStorage { get; set; }
        public int UrlLength { get; set; }
        public int DeleteKeyLength { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public bool IncludeExtension { get; set; }
        // The size of the chunk that the file will be encrypted/decrypted in (bytes)
        public int ChunkSize { get; set; }
        // Virus Scanning Settings
        public ClamConfig ClamConfig { get; set; }
        // Hash Scanning Settings
        public HashScanConfig HashScanConfig { get; set; }
        // Storage settings
        public StorageConfig StorageConfig { get; set; }
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
            MaxUploadFileSize = 1073741824;
            MaxDownloadFileSize = 1073741824;
            MaxStorage = 5368709120;
            UrlLength = 5;
            DeleteKeyLength = 24;
            KeySize = 256;
            BlockSize = 128;
            IncludeExtension = true;
            ChunkSize = 1024;
            ClamConfig = new ClamConfig();
            HashScanConfig = new HashScanConfig();
            StorageConfig = new StorageConfig("uploads");
            RestrictedContentTypes = new List<string>();
            RestrictedExtensions = new List<string>();
        }
    }
}
