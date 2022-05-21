using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Attributes;

namespace Teknik.Areas.Upload.Models
{
    [Index(nameof(Url))]
    public class Upload
    {
        public int UploadId { get; set; }
        
        public int? UserId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Vault.Models.UploadVaultItem> UploadVaultItems { get; set; }

        public DateTime DateUploaded { get; set; }

        [MaxLength(250)]
        [CaseSensitive]
        public string Url { get; set; }

        [CaseSensitive]
        public string FileName { get; set; }

        public long ContentLength { get; set; }

        public string ContentType { get; set; }

        [CaseSensitive]
        public string Key { get; set; }

        [CaseSensitive]
        public string IV { get; set; }

        public int KeySize { get; set; }

        public int BlockSize { get; set; }

        [CaseSensitive]
        public string DeleteKey { get; set; }

        public DateTime? ExpireDate { get; set; }

        public int MaxDownloads { get; set; }

        public int Downloads { get; set; }
    }
}