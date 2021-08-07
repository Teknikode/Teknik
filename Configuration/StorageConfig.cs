using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class StorageConfig
    {
        public StorageType Type { get; set; }
        // File Extension for saved files
        public string FileExtension { get; set; }
        // Length of filename
        public int FileNameLength { get; set; }

        // Local Storage Options
        public string LocalDirectory { get; set; }

        // S3 Options
        public string Container { get; set; }

        public StorageConfig(string container)
        {
            Type = StorageType.Local;
            Container = container;
            LocalDirectory = Directory.GetCurrentDirectory();
            FileExtension = "enc";
            FileNameLength = 10;
        }
    }
}
