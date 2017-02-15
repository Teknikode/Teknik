using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Vault.Models
{
    public class PasteVaultItem : VaultItem
    {
        public int PasteId { get; set; }
        public virtual Paste.Models.Paste Paste { get; set; }
    }
}