using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Vault.Models
{
    public class UploadVaultItem : VaultItem
    {
        public int UploadId { get; set; }
        public virtual Upload.Models.Upload Upload { get; set; }
    }
}