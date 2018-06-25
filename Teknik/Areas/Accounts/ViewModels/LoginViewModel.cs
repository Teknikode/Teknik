using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teknik.ViewModels;

namespace Teknik.Areas.Accounts.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}