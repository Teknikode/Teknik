using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Utilities;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class UploadSettingsViewModel : SettingsViewModel
    {
        public long MaxStorage { get; set; }
        public long MaxFileSize { get; set; }

        public bool Encrypt { get; set; }
        public int ExpirationLength { get; set; }
        public ExpirationUnit ExpirationUnit { get; set; }

        public UploadSettingsViewModel()
        {
            Encrypt = false;
            ExpirationLength = 1;
            ExpirationUnit = ExpirationUnit.Never;
        }
    }
}
