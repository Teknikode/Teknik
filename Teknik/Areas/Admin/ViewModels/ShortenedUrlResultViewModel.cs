using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.ViewModels
{
    public class ShortenedUrlResultViewModel : ViewModelBase
    {
        public string OriginalUrl { get; set; }
        public string ShortenedUrl { get; set; }
        public DateTime CreationDate { get; set; }
        public int Views { get; set; }
        public string Username { get; set; }
    }
}
