using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.ViewModels
{
    public class PasteResultViewModel : ViewModelBase
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public DateTime DatePosted { get; set; }
        public int Views { get; set; }
        public string DeleteKey { get; set; }
        public string Username { get; set; }
    }
}
