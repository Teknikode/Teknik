using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Teknik.Attributes;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Users.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        public string Username { get; set; }

        [CaseSensitive]
        public string HashedPassword { get; set; }

        public virtual ICollection<TransferType> Transfers { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public AccountType AccountType { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
        
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
            HashedPassword = string.Empty;
            Transfers = new List<TransferType>();
            JoinDate = DateTime.Now;
            LastSeen = DateTime.Now;
            AccountType = AccountType.Basic;
            Groups = new List<Group>();
            TrustedDevices = new List<TrustedDevice>();
            AuthTokens = new List<AuthToken>();
        }
    }
}