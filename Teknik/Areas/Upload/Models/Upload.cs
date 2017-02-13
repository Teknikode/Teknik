using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Attributes;

namespace Teknik.Areas.Upload.Models
{
    public class Upload
    {
        public int UploadId { get; set; }
        
        public int? UserId { get; set; }

        public virtual User User { get; set; }

        public int? VaultId { get; set; }

        public virtual Vault.Models.Vault Vault { get; set; }

        public DateTime DateUploaded { get; set; }

        [CaseSensitive]
        public string Url { get; set; }

        [CaseSensitive]
        public string FileName { get; set; }

        public int ContentLength { get; set; }

        public string ContentType { get; set; }

        [CaseSensitive]
        public string Key { get; set; }

        [CaseSensitive]
        public string IV { get; set; }

        public int KeySize { get; set; }

        public int BlockSize { get; set; }

        [CaseSensitive]
        public string DeleteKey { get; set; }

        public int Downloads { get; set; }
    }
}