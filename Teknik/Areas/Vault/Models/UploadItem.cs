using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Vault.Models
{
    public class UploadItem : VaultItem
    {
        public int UploadId { get; set; }
        public Upload.Models.Upload Upload { get; set; }
    }
}