using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace Teknik.Areas.Profile.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        public string Username { get; set; }

        public string HashedPassword { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public List<Group> Groups { get; set; }

        public string About { get; set; }

        public string Website { get; set; }

        public string Quote { get; set; }

        public User()
        {
            Username = String.Empty;
            HashedPassword = String.Empty;
            JoinDate = DateTime.Now;
            LastSeen = DateTime.Now;
            Groups = new List<Group>();
            About = string.Empty;
            Website = string.Empty;
            Quote = string.Empty;
        }
    }
}