using System;
using System.ComponentModel.DataAnnotations;
using Teknik.Areas.Contact.Models;
using System.Linq;
using System.Web;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Contact.ViewModels
{
    public class ContactViewModel : ViewModelBase
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
}