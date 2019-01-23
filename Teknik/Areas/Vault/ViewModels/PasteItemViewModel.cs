using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class PasteItemViewModel : VaultItemViewModel
    {
        public int PasteId { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public string Syntax { get; set; }
        public DateTime DatePosted { get; set; }
        public bool HasPassword { get; set; }
    }
}