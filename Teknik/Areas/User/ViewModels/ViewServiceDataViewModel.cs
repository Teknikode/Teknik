using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class ViewServiceDataViewModel : ViewModelBase
    {
        public DateTime LastSeen { get; set; }

        public long MaxUploadStorage { get; set; }

        public long CurrentUploadStorage { get; set; }

        public List<Upload.Models.Upload> Uploads { get; set; }

        public List<Paste.Models.Paste> Pastes { get; set; }

        public List<Shortener.Models.ShortenedUrl> ShortenedUrls { get; set; }

        public List<Vault.Models.Vault> Vaults { get; set; }
    }
}
