using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Users.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class ProfileSettingsViewModel : SettingsViewModel
    {
        [AllowHtml]
        public string About { get; set; }

        public string Website { get; set; }

        public string Quote { get; set; }

        public ProfileSettingsViewModel()
        {
            About = string.Empty;
            Website = string.Empty;
            Quote = string.Empty;
        }
    }
}
