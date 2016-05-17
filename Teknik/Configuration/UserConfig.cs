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
        public string ReservedUsernameDefinitionFile { get; set; }

        public UserConfig()
        {
            RegistrationEnabled = true;
            LoginEnabled = true;
            ReservedUsernameDefinitionFile = string.Empty;
        }
    }
}
