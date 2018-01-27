using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.ViewModels;

namespace Teknik.Areas.Error.ViewModels
{
    public class SubmitReportViewModel : ViewModelBase
    {
        [AllowHtml]
        public string Message { get; set; }

        [AllowHtml]
        public string Exception { get; set; }

        [AllowHtml]
        public string CurrentUrl { get; set; }
    }
}
