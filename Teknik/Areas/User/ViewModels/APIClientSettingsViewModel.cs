using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.ViewModels
{
    public class APIClientSettingsViewModel : SettingsViewModel
    {

        public List<AuthTokenViewModel> AuthTokens { get; set; }

        public APIClientSettingsViewModel()
        {
            AuthTokens = new List<AuthTokenViewModel>();
        }
    }
}
