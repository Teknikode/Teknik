using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using Teknik.Helpers;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class LoginViewModel : ViewModelBase
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

        public string ReturnUrl { get; set; }
    }
}