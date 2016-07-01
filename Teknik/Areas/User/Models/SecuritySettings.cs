using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Teknik.Attributes;

namespace Teknik.Areas.Users.Models
{
    public class SecuritySettings
    {
        [Key]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual UserSettings UserSettings { get; set; }

        public virtual BlogSettings BlogSettings { get; set; }

        public virtual UploadSettings UploadSettings { get; set; }

        public string RecoveryEmail { get; set; }

        public bool RecoveryVerified { get; set; }

        public bool AllowTrustedDevices { get; set; }

        public bool TwoFactorEnabled { get; set; }

        [CaseSensitive]
        public string TwoFactorKey { get; set; }

        public string PGPSignature { get; set; }

        public SecuritySettings()
        {
            RecoveryEmail = string.Empty;
            RecoveryVerified = false;
            AllowTrustedDevices = false;
            TwoFactorEnabled = false;
            TwoFactorKey = string.Empty;
            PGPSignature = string.Empty;
        }
    }
}