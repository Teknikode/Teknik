using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Utilities;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastSeen { get; set; }

        public AccountType AccountType { get; set; }

        public AccountStatus AccountStatus { get; set; }

        public List<Upload.Models.Upload> Uploads { get; set; }

        public List<Paste.Models.Paste> Pastes { get; set; }

        public List<Shortener.Models.ShortenedUrl> ShortenedUrls { get; set; }

        public List<Vault.Models.Vault> Vaults { get; set; }

        public UserSettings UserSettings { get; set; }

        public SecuritySettings SecuritySettings { get; set; }

        public BlogSettings BlogSettings { get; set; }

        public UploadSettings UploadSettings { get; set; }
    }
}
