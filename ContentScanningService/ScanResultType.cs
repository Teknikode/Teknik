using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.ContentScanningService
{
    public enum ScanResultType
    {
        Unknown = 0,
        Clean = 1,
        VirusDetected = 2,
        ChildPornography = 3,
        Error = 4
    }
}
