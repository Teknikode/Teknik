using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Teknik.Attributes;
using Teknik.Models;

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

        public virtual ICollection<Group> Groups { get; set; }
        
        public virtual UserSettings UserSettings { get; set; }

        public virtual SecuritySettings SecuritySettings { get; set; }

        public virtual BlogSettings BlogSettings { get; set; }
        
        public virtual UploadSettings UploadSettings { get; set; }

        public virtual ICollection<UserDevice> Devices { get; set; }

        public virtual ICollection<Upload.Models.Upload> Uploads { get; set; }

        public virtual ICollection<Paste.Models.Paste> Pastes { get; set; }

        public User()
        {
            Username = string.Empty;
            HashedPassword = string.Empty;
            Transfers = new List<TransferType>();
            JoinDate = DateTime.Now;
            LastSeen = DateTime.Now;
            Groups = new List<Group>();
            Devices = new List<UserDevice>();
        }
    }
}