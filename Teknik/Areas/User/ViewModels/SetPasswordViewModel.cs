using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teknik.Areas.Users.ViewModels
{
    public class SetPasswordViewModel
    {
        [AllowHtml]
        public string Password { get; set; }

        [AllowHtml]
        public string PasswordConfirm { get; set; }
    }
}