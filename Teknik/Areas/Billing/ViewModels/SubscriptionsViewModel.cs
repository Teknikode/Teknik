using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class SubscriptionsViewModel : ViewModelBase
    {
        public List<SubscriptionViewModel> UploadSubscriptions { get; set; }
        public List<SubscriptionViewModel> EmailSubscriptions { get; set; }

        public SubscriptionsViewModel()
        {
            UploadSubscriptions = new List<SubscriptionViewModel>();
            EmailSubscriptions = new List<SubscriptionViewModel>();
        }
    }
}
