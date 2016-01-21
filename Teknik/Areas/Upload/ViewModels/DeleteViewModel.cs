using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Upload.ViewModels
{
    public class DeleteViewModel : ViewModelBase
    {
        public string File { get; set; }
        public bool Deleted { get; set; }
    }
}