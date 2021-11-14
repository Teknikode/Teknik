using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.BillingCore.Models;

namespace Teknik.Areas.Users.ViewModels
{
    public class BillingSettingsViewModel : SettingsViewModel
    {
        public List<SubscriptionViewModel> Subscriptions { get; set; }

        public BillingSettingsViewModel()
        {
            Subscriptions = new List<SubscriptionViewModel>();
        }
    }
}
