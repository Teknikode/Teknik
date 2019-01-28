using System;
using System.Collections.Generic;

namespace Teknik.Areas.Users.ViewModels
{
    public class SecuritySettingsViewModel : SettingsViewModel
    {
        public string PgpPublicKey { get; set; }

        public string RecoveryEmail { get; set; }

        public bool RecoveryVerified { get; set; }

        public bool AllowTrustedDevices { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public SecuritySettingsViewModel()
        {
            RecoveryEmail = string.Empty;
            RecoveryVerified = false;
            AllowTrustedDevices = false;
            TwoFactorEnabled = false;
            PgpPublicKey = string.Empty;
        }
    }
}
