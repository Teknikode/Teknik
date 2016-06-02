using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Users.Models
{
    public class ResetPasswordVerification
    {
        public int ResetPasswordVerificationId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public string Code { get; set; }

        public DateTime DateCreated { get; set; }
    }
}