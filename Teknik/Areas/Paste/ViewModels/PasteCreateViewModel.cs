using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.ViewModels;

namespace Teknik.Areas.Paste.ViewModels
{
    public class PasteCreateViewModel : ViewModelBase
    {
        [Required]
        [AllowHtml]
        public string Content { get; set; }

        public string Title { get; set; }

        public string Syntax { get; set; }

        [Range(1, int.MaxValue)]
        public int? ExpireLength { get; set; }

        public string ExpireUnit { get; set; }

        [AllowHtml]
        public string Password { get; set; }

        public bool Hide { get; set; }
    }
}