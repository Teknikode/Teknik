using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.ViewModels
{
    public class LoginViewModel
    {
        private TeknikEntities db = new TeknikEntities();

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public bool IsValid()
        {
            return IsValid(Username, Password);
        }

        public bool IsValid(string username, string password)
        {
            var myUser = db.Users.Where(b => b.Username == username).FirstOrDefault();

            if (myUser != null && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                return (myUser.HashedPassword == SHA384.Hash(username, password));
            }
            return false;
        }
    }
}