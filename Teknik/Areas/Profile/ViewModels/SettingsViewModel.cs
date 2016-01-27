using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Profile.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Profile.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        public UserSettings UserSettings { get; set; }

        public BlogSettings BlogSettings { get; set; }

        public UploadSettings UploadSettings { get; set; }
    }
}