using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Upload.ViewModels
{
    public class DownloadViewModel : ViewModelBase
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public string IV { get; set; }
        public int keySize { get; set; }
        public int blockSize { get; set; }
    }
}