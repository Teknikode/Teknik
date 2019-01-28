namespace Teknik.Areas.Users.ViewModels
{
    public class ProfileSettingsViewModel : SettingsViewModel
    {
        public string About { get; set; }

        public string Website { get; set; }

        public string Quote { get; set; }

        public ProfileSettingsViewModel()
        {
            About = string.Empty;
            Website = string.Empty;
            Quote = string.Empty;
        }
    }
}
