using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Vault.Models
{
    public class VaultItem
    {
        public int VaultItemId { get; set; }
        public int VaultId { get; set; }
        public virtual Vault Vault { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
    }
}