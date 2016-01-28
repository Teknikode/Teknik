using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class UploadConfig
    {
        public bool UploadEnabled { get; set; }
        public bool DownloadEnabled { get; set; }
        // Max upload size in bytes
        public int MaxUploadSize { get; set; }
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

        public UploadConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            UploadEnabled = true;
            DownloadEnabled = true;
            MaxUploadSize = 100000000;
            UploadDirectory = Directory.GetCurrentDirectory();
            FileExtension = "enc";
            UrlLength = 5;
            DeleteKeyLength = 24;
            KeySize = 256;
            BlockSize = 128;
            IncludeExtension = true;
            ChunkSize = 1024;
        }
    }
}