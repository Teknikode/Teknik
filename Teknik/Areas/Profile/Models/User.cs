using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teknik.Areas.Profile.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        public string Username { get; set; }

        public string HashedPassword { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public List<Group> Groups { get; set; }
        
        [Required]
        public virtual UserSettings UserSettings { get; set; }

        [Required]
        public virtual BlogSettings BlogSettings { get; set; }
        
        [Required]
        public virtual UploadSettings UploadSettings { get; set; }

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