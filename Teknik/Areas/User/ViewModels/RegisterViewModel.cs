using System;
using System.ComponentModel.DataAnnotations;
using Teknik.Areas.Users.Models;
using Teknik.Helpers;
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

        public string ReturnUrl { get; set; }
    }
}