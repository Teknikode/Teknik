using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class UserConfig
    {
        public bool RegistrationEnabled { get; set; }
        public bool LoginEnabled { get; set; }
        public string UsernameFilter { get; set; }
        public string UsernameFilterLabel { get; set; }
        public int MinUsernameLength { get; set; }
        public int MaxUsernameLength { get; set; }
        public string ReservedUsernameDefinitionFile { get; set; }

        public UserConfig()
        {
            RegistrationEnabled = true;
            LoginEnabled = true;
            UsernameFilter = "^[a-zA-Z0-9_-]+(?:\\.[a-zA-Z0-9_-]+)*$";
            UsernameFilterLabel = "AlphaNumeric Characters with Dashes, Underlines, and 0-1 Periods not in the beginning or end.";
            MinUsernameLength = 1;
            MaxUsernameLength = 35;
            ReservedUsernameDefinitionFile = string.Empty;
        }
    }
}
