using Teknik.IdentityServer.Models;

namespace Teknik.IdentityServer.ViewModels
{
    public class LoginViewModel : LoginInputModel
    {
        public bool AllowRememberLogin { get; set; }
        public bool EnableLocalLogin { get; set; }
    }
}
