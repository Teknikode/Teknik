using System;
using System.ComponentModel.DataAnnotations;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.ViewModels
{
    public class RegisterViewModel
    {
        private TeknikEntities db = new TeknikEntities();

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public bool Insert()
        {
            bool success = true;
            try
            {
                User newUser = db.Users.Create();
                newUser.JoinDate = DateTime.Now;
                newUser.Username = Username;
                newUser.HashedPassword = SHA384.Hash(Username, Password);
                db.Users.Add(newUser);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }
    }
}