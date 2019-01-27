using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Teknik.Attributes;
using Teknik.Models;
using Teknik.Utilities;
using Microsoft.AspNetCore.Identity;

namespace Teknik.Areas.Users.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public virtual ICollection<UserFeature> Features { get; set; }

        public virtual InviteCode ClaimedInviteCode { get; set; }

        public virtual ICollection<InviteCode> OwnedInviteCodes { get; set; }
        
        public virtual UserSettings UserSettings { get; set; }

        public virtual BlogSettings BlogSettings { get; set; }
        
        public virtual UploadSettings UploadSettings { get; set; }

        public virtual ICollection<Upload.Models.Upload> Uploads { get; set; }

        public virtual ICollection<Paste.Models.Paste> Pastes { get; set; }

        public virtual ICollection<Shortener.Models.ShortenedUrl> ShortenedUrls { get; set; }

        public virtual ICollection<Vault.Models.Vault> Vaults { get; set; }

        public User()
        {
            Username = string.Empty;
            Features = new List<UserFeature>();
            ClaimedInviteCode = null;
            OwnedInviteCodes = new List<InviteCode>();
        }
    }
}
