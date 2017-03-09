using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.ViewModels
{
    public class UploadResultViewModel : ViewModelBase
    {
        public string Url { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public DateTime DateUploaded { get; set; }
        public int Downloads { get; set; }
        public string DeleteKey { get; set; }
    }
}