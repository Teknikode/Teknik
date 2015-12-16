using System;
using System.ComponentModel.DataAnnotations;
using Teknik.Areas.Contact.Models;
using System.Linq;
using System.Web;
using Teknik.Models;

namespace Teknik.Areas.Contact.ViewModels
{
    public class ContactViewModel
    {
        private TeknikEntities db = new TeknikEntities();

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

        public bool Insert()
        {
            bool success = true;
            try
            {
                Models.Contact newContact = db.Contact.Create();
                newContact.Name = Name;
                newContact.Email = Email;
                newContact.Subject = Subject;
                newContact.Message = Message;
                newContact.DateAdded = DateTime.Now;
                db.Contact.Add(newContact);
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