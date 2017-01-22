using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        public int TrustedDeviceCount { get; set; }

        public List<AuthTokenViewModel> AuthTokens { get; set; }

        public UserSettings UserSettings { get; set; }

        public SecuritySettings SecuritySettings { get; set; }

        public BlogSettings BlogSettings { get; set; }

        public UploadSettings UploadSettings { get; set; }
    }
}