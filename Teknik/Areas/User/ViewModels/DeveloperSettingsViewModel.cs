using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.ViewModels
{
    public class DeveloperSettingsViewModel : SettingsViewModel
    {

        public List<AuthTokenViewModel> AuthTokens { get; set; }
        public List<ClientViewModel> Clients { get; set; }

        public DeveloperSettingsViewModel()
        {
            AuthTokens = new List<AuthTokenViewModel>();
            Clients = new List<ClientViewModel>();
        }
    }
}
