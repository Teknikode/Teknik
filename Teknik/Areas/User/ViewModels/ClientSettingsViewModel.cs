using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.ViewModels
{
    public class ClientSettingsViewModel : SettingsViewModel
    {
        public List<ClientViewModel> Clients { get; set; }

        public ClientSettingsViewModel()
        {
            Clients = new List<ClientViewModel>();
        }
    }
}
