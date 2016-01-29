using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teknik.Areas.Profile.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        public string Username { get; set; }

        public string HashedPassword { get; set; }

        public bool TransferAccount { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public List<Group> Groups { get; set; }
        
        public virtual UserSettings UserSettings { get; set; }
        
        public virtual BlogSettings BlogSettings { get; set; }
        
        public virtual UploadSettings UploadSettings { get; set; }

        public List<Upload.Models.Upload> Uploads { get; set; }

        public List<Paste.Models.Paste> Pastes { get; set; }

        public User()
        {
            Username = String.Empty;
            HashedPassword = String.Empty;
            JoinDate = DateTime.Now;
            LastSeen = DateTime.Now;
            Groups = new List<Group>();
            //UserSettings = new UserSettings();
            //BlogSettings = new BlogSettings();
            //UploadSettings = new UploadSettings();
        }
    }
}