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

        public bool Encrypt { get; set; }

        public List<Vault.Models.Vault> Vaults { get; set; }

        public UploadViewModel()
        {
            CurrentSub = string.Empty;
            Encrypt = false;
            Vaults = new List<Vault.Models.Vault>();
        }
    }
}