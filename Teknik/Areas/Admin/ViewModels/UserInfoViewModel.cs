using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Utilities;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.ViewModels
{
    public class UserInfoViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public AccountType AccountType { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public string Email { get; set; }
        public bool EmailEnabled { get; set; }
        public long MaxEmailStorage { get; set; }
    }
}
