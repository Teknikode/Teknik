using System;
using System.Collections.Generic;

namespace Teknik.Areas.Users.ViewModels
{
    public class SecuritySettingsViewModel : SettingsViewModel
    {
        public string CurrentPassword { get; set; }
        
        public string NewPassword { get; set; }
        
        public string NewPasswordConfirm { get; set; }

        public string PgpPublicKey { get; set; }

        public string RecoveryEmail { get; set; }

        public bool RecoveryVerified { get; set; }

        public bool AllowTrustedDevices { get; set; }

        public bool TwoFactorEnabled { get; set; }
        
        public string TwoFactorKey { get; set; }

        public int TrustedDeviceCount { get; set; }

        public List<AuthTokenViewModel> AuthTokens { get; set; }

        public SecuritySettingsViewModel()
        {
            TrustedDeviceCount = 0;
            AuthTokens = new List<AuthTokenViewModel>();
            RecoveryEmail = string.Empty;
            RecoveryVerified = false;
            AllowTrustedDevices = false;
            TwoFactorEnabled = false;
            TwoFactorKey = string.Empty;
            PgpPublicKey = string.Empty;
        }
    }
}
