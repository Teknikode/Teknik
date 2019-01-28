namespace Teknik.Areas.Users.ViewModels
{
    public class EditSettingsViewModel
    {
        public string CurrentPassword { get; set; }
        
        public string NewPassword { get; set; }
        
        public string NewPasswordConfirm { get; set; }
        
        public string PgpPublicKey { get; set; }
        
        public string RecoveryEmail { get; set; }
        
        public bool AllowTrustedDevices { get; set; }
        
        public bool TwoFactorEnabled { get; set; }
        
        public string Website { get; set; }
        
        public string Quote { get; set; }
        
        public string About { get; set; }
        
        public string BlogTitle { get; set; }
        
        public string BlogDesc { get; set; }
        
        public bool Encrypt { get; set; }
    }
}
