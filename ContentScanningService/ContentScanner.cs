using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.ContentScanningService
{
    public abstract class ContentScanner
    {
        protected readonly Config _config;

        public ContentScanner(Config config)
        {
            _config = config;
        }

        public abstract Task<ScanResult> ScanFile(Stream stream);
    }
}
