using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Profile.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public string About { get; set; }

        public string Website { get; set; }

        public string Quote { get; set; }

        public string BlogTitle { get; set; }

        public string BlogDescription { get; set; }
    }
}