using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Attributes;
using Teknik.Models;

namespace Teknik.Areas.Paste.Models
{
    public class Paste
    {
        public int PasteId { get; set; }

        public int? UserId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Vault.Models.PasteVaultItem> PasteVaultItems { get; set; }

        public DateTime DatePosted { get; set; }

        [CaseSensitive]
        public string Url { get; set; }

        [CaseSensitive]
        public string FileName { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public string Syntax { get; set; }

        public DateTime? ExpireDate { get; set; }

        [CaseSensitive]
        public string HashedPassword { get; set; }

        [CaseSensitive]
        public string Key { get; set; }

        public int KeySize { get; set; }

        [CaseSensitive]
        public string IV { get; set; }

        public int BlockSize { get; set; }

        [CaseSensitive]
        public string DeleteKey { get; set; }

        public bool Hide { get; set; }

        public int MaxViews { get; set; }

        public int Views { get; set; }
    }
}
