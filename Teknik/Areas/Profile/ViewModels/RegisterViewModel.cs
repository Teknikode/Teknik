using System;
using System.ComponentModel.DataAnnotations;
using Teknik.Areas.Profile.Models;
using Teknik.Helpers;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Profile.ViewModels
{
    public class RegisterViewModel : ViewModelBase
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
    }
}