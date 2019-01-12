using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RedirectURI { get; set; }
        public string PostLogoutRedirectURI { get; set; }
        public ICollection<string> AllowedScopes { get; set; }
    }
}
