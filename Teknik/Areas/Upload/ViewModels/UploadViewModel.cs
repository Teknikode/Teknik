using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Utilities;
using Teknik.ViewModels;

namespace Teknik.Areas.Upload.ViewModels
{
    public class UploadViewModel : ViewModelBase
    {
        public string CurrentSub { get; set; }

        public bool Encrypt { get; set; }

        public int ExpirationLength { get; set; }

        public ExpirationUnit ExpirationUnit { get; set; }

        public List<Vault.Models.Vault> Vaults { get; set; }

        public long MaxUploadSize { get; set; }

        public long MaxTotalSize { get; set; }

        public long CurrentTotalSize { get; set; }

        public UploadViewModel()
        {
            CurrentSub = string.Empty;
            Encrypt = false;
            ExpirationLength = 1;
            ExpirationUnit = ExpirationUnit.Never;
            Vaults = new List<Vault.Models.Vault>();
            MaxUploadSize = 0;
            MaxTotalSize = -1;
            CurrentTotalSize = 0;
        }
    }
}