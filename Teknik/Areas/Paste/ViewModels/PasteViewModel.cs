using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teknik.ViewModels;

namespace Teknik.Areas.Paste.ViewModels
{
    public class PasteViewModel : ViewModelBase
    {
        public string Url { get; set; }
        [AllowHtml]
        public string Content { get; set; }
        public string Title { get; set; }
        public string Syntax { get; set; }
        [AllowHtml]
        public string Password { get; set; }
        public bool Hide { get; set; }
        public DateTime DatePosted { get; set; }

        public List<Vault.Models.Vault> Vaults { get; set; }
    }
}
