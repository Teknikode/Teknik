using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.ViewModels
{
    public class UserResultViewModel : ViewModelBase
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }
    }
}
