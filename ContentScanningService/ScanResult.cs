using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Teknik.ContentScanningService
{
    public class ScanResult
    {
        public string RawResult { get; set;}
        public ScanResultType ResultType { get; set; }

        public ScanResult()
        {
            RawResult = string.Empty;
            ResultType = ScanResultType.Clean;
        }
    }
}
