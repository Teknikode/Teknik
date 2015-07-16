using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Teknik.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        public string Username { get; set; }

        public string HashedPassword { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public User()
        {
            Username = String.Empty;
            HashedPassword = String.Empty;
            JoinDate = DateTime.Now;
            LastSeen = DateTime.Now;
        }
    }
}