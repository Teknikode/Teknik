using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class UploadItemViewModel : VaultItemViewModel
    {
        public Upload.Models.Upload Upload { get; set; }
    }
}