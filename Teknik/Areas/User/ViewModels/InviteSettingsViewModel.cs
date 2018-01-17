using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class InviteSettingsViewModel : SettingsViewModel
    {
        public List<InviteCodeViewModel> AvailableCodes { get; set; }
        public List<InviteCodeViewModel> ClaimedCodes { get; set; }

        public InviteSettingsViewModel()
        {
            AvailableCodes = new List<InviteCodeViewModel>();
            ClaimedCodes = new List<InviteCodeViewModel>();
        }
    }
}
