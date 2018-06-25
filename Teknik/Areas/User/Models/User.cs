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

        [NotMapped]
        public string Password { get; set; }

        [CaseSensitive]
        public string HashedPassword { get; set; }

        public virtual ICollection<LoginInfo> Logins { get; set; }

        public virtual ICollection<TransferType> Transfers { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public virtual InviteCode ClaimedInviteCode { get; set; }

        public virtual ICollection<InviteCode> OwnedInviteCodes { get; set; }

        public AccountType AccountType { get; set; }

        public AccountStatus AccountStatus { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
        
        public virtual UserSettings UserSettings { get; set; }

        public virtual SecuritySettings SecuritySettings { get; set; }

        public virtual BlogSettings BlogSettings { get; set; }
        
        public virtual UploadSettings UploadSettings { get; set; }

        public virtual ICollection<TrustedDevice> TrustedDevices { get; set; }

        public virtual ICollection<AuthToken> AuthTokens { get; set; }

        public virtual ICollection<Upload.Models.Upload> Uploads { get; set; }

        public virtual ICollection<Paste.Models.Paste> Pastes { get; set; }

        public virtual ICollection<Shortener.Models.ShortenedUrl> ShortenedUrls { get; set; }

        public virtual ICollection<Vault.Models.Vault> Vaults { get; set; }

        public User()
        {
            Username = string.Empty;
            Password = string.Empty;
            HashedPassword = string.Empty;
            Logins = new List<LoginInfo>();
            Transfers = new List<TransferType>();
            JoinDate = DateTime.Now;
            LastSeen = DateTime.Now;
            AccountType = AccountType.Basic;
            AccountStatus = AccountStatus.Active;
            UserRoles = new List<UserRole>();
            TrustedDevices = new List<TrustedDevice>();
            AuthTokens = new List<AuthToken>();
            ClaimedInviteCode = null;
            OwnedInviteCodes = new List<InviteCode>();
        }
    }
}
