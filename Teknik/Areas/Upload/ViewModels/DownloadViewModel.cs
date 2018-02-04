using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Upload.ViewModels
{
    public class DownloadViewModel : ViewModelBase
    {
        public string CurrentSub { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string IV { get; set; }
        public bool Decrypt { get; set; }
    }
}
