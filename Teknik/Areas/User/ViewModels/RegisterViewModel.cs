using System;
using System.ComponentModel.DataAnnotations;
using Teknik.Areas.Users.Models;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
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

        [Display(Name = "Recovery Email")]
        public string RecoveryEmail { get; set; }

        [Display(Name = "Public PGP Key")]
        public string PublicKey { get; set; }

        public string ReturnUrl { get; set; }
    }
}