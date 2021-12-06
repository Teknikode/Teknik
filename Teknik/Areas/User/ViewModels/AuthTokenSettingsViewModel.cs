using System.Collections.Generic;

namespace Teknik.Areas.Users.ViewModels
{
    public class AuthTokenSettingsViewModel : SettingsViewModel
    {
        public List<AuthTokenViewModel> AuthTokens { get; set; }

        public AuthTokenSettingsViewModel()
        {
            AuthTokens = new List<AuthTokenViewModel>();
        }
    }
}
