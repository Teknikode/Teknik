using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class PasteItemViewModel : VaultItemViewModel
    {
        public Paste.Models.Paste Paste { get; set; }
    }
}