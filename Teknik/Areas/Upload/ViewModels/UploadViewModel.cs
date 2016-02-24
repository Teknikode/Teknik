using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Upload.ViewModels
{
    public class UploadViewModel : ViewModelBase
    {
        public string CurrentSub { get; set; }

        public bool SaveKey { get; set; }

        public bool ServerSideEncrypt { get; set; }
    }
}