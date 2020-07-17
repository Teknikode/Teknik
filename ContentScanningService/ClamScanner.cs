using nClam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.ContentScanningService
{
    public class ClamScanner : ContentScanner
    {
        public ClamScanner(Config config) : base(config)
        { }

        public async override Task<ScanResult> ScanFile(Stream stream)
        {
            var result = new ScanResult();
            if (stream != null)
            {
                // Set the start of the stream
                stream.Seek(0, SeekOrigin.Begin);

                ClamClient clam = new ClamClient(_config.UploadConfig.ClamConfig.Server, _config.UploadConfig.ClamConfig.Port);
                clam.MaxStreamSize = stream.Length;
                ClamScanResult scanResult = await clam.SendAndScanFileAsync(stream);

                result.RawResult = scanResult.RawResult;
                switch (scanResult.Result)
                {
                    case ClamScanResults.Clean:
                        result.ResultType = ScanResultType.Clean;
                        break;
                    case ClamScanResults.VirusDetected:
                        result.ResultType = ScanResultType.VirusDetected;
                        result.RawResult = scanResult.InfectedFiles.First().VirusName;
                        break;
                    case ClamScanResults.Error:
                        result.ResultType = ScanResultType.Error;
                        break;
                    case ClamScanResults.Unknown:
                        result.ResultType = ScanResultType.Unknown;
                        break;
                }
            }
            return result;
        }
    }
}
