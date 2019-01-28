using Teknik.IdentityServer.Models;

namespace Teknik.IdentityServer.ViewModels
{
    public class LogoutViewModel : LogoutInputModel
    {
        public bool ShowLogoutPrompt { get; set; }
    }
}
